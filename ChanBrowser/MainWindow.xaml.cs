using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ChanBrowserLibrary;

namespace ChanBrowser
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        System.Threading.CancellationTokenSource tokenSource = new System.Threading.CancellationTokenSource();

        public MainWindow()
        {
            InitializeComponent();
            WindowState = WindowState.Maximized;
            KeyDown += (object s, KeyEventArgs e) => 
            {
                if (e.Key == Key.OemTilde)
                    MessageBox.Show("Dev console not avaliable!");
            };
            loadBoardList();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            tokenSource.Cancel();
            base.OnClosing(e);
        }
        
        private void loadBoardList()
        {
            Task<List<Tuple<string, string>>> boardListTask = Global.getBoardList(tokenSource.Token);
            boardListTask.ContinueWith(t =>
            {
                switch (t.Status)
                {
                    case TaskStatus.RanToCompletion:
                        foreach (var board in t.Result)
                        {
                            Button boardButton = new Button();
                            boardButton.MinWidth = 100;
                            boardButton.Margin = new Thickness(1);
                            boardButton.Content = board.Item1;
                            boardButton.Click += BoardButton_Click;
                            BoardList.Children.Add(boardButton);

                            ToolTip toolTip = new ToolTip();
                            toolTip.Content = board.Item2;
                            ToolTipService.SetToolTip(boardButton, toolTip);
                        }
                        break;
                    case TaskStatus.Canceled:
                        MessageBox.Show("getBoardList was canceled!", "CANCELED");
                        break;
                    case TaskStatus.Faulted:
                        MessageBox.Show("getBoardList was faulted!", "EXCEPTION");
                        break;
                }
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private async void BoardButton_Click(object sender, RoutedEventArgs e)
        {
            if (((Button)sender).Content.ToString().Equals("~null~"))
            {
                Title = "~~~DEV BUTTON~~~";
                for (int i = 0; i < MainGrid.Children.OfType<Image>().Count(); i++)
                {
                    Image image = MainGrid.Children.OfType<Image>().ToArray()[i];
                    image.Source = new BitmapImage(new Uri("https://cdn.sstatic.net/Sites/stackoverflow/img/apple-touch-icon@2.png"));
                }
            }
            else
            {
                foreach (var item in BoardGrid.Children.OfType<StackPanel>())
                {
                    item.Children.OfType<Image>().First().Source = new BitmapImage(new Uri(Global.DEFAULT_IMAGE));
                    item.Children.OfType<TextBlock>().First().Text = "";
                    item.Children.OfType<TextBlock>().Last().Text = "";

                    item.DataContext = null;
                }

                Task task = Global.loadBoard(((Button)sender).Content.ToString(), tokenSource.Token);
                await task.ContinueWith(t =>
                 {
                     switch (t.Status)
                     {
                         case TaskStatus.RanToCompletion:
                             Title = "/" + Global.currentBoard + "/";

                             foreach (var item in BoardGrid.Children.OfType<StackPanel>()
                                    .Zip(Global.chanThreadList,
                                    (i, b) => new { ChanPanel = i, ChanThread = b }))
                             {
                                 //item.ChanPanel.Children.OfType<Image>().First().MaxHeight =
                                 //   (((Grid)item.ChanPanel.Parent).ActualHeight / ((Grid)item.ChanPanel.Parent).RowDefinitions.Count) * .8;

                                 BitmapImage bitmapImage = new BitmapImage(new Uri(item.ChanThread.imageUrl));
                                 item.ChanPanel.Children.OfType<Image>().First().Source = bitmapImage;

                                 bitmapImage.DownloadCompleted += (ds, de) =>
                                 {
                                     item.ChanPanel.Children.OfType<Image>().First().MaxHeight = bitmapImage.PixelHeight;
                                     item.ChanPanel.Children.OfType<Image>().First().MaxWidth = bitmapImage.PixelWidth;
                                 };

                                 item.ChanPanel.DataContext = item.ChanThread;

                                 Global.htmlToTextBlockText(item.ChanPanel.Children.OfType<TextBlock>().First(),
                                     "<strong>R:" + item.ChanThread.replies + "/I:" + item.ChanThread.images + "\n" +
                                     System.Net.WebUtility.HtmlDecode(item.ChanThread.sub) + "</strong>");
                                 Global.htmlToTextBlockText(item.ChanPanel.Children.OfType<TextBlock>().Last(),
                                     System.Net.WebUtility.HtmlDecode(item.ChanThread.com));

                                 
                             }
                             break;
                         case TaskStatus.Canceled:
                             MessageBox.Show("loadBoard was canceled!", "CANCELED");
                             break;
                         case TaskStatus.Faulted:
                             MessageBox.Show("loadBoard was faulted!", "EXCEPTION");
                             break;
                     }
                 }, TaskScheduler.FromCurrentSynchronizationContext());
            }
        }

        private void ThreadPanel_MouseUp(object sender, RoutedEventArgs e)
        {
            if (((StackPanel)sender).DataContext != null)
            {
                ChanPost chanPost = ((ChanPost)((StackPanel)sender).DataContext);
                Button threadButton = new Button();
                threadButton.MinWidth = 100;
                threadButton.Margin = new Thickness(1);
                threadButton.Content = chanPost.no.ToString() + "\n" + chanPost.semantic_url;
                threadButton.DataContext = ((FrameworkElement)sender).DataContext;
                threadButton.Click += ThreadButton_Click;

                ThreadList.Children.Add(threadButton); 
            }
        }
        
        private async void ThreadButton_Click(object sender, RoutedEventArgs e)
        {
            ChanPost op = ((ChanPost)((FrameworkElement)sender).DataContext);

            ThreadStackPanel.Children.Clear();

            Task loadThread = Global.loadThread(op, tokenSource.Token);
            await loadThread.ContinueWith(t =>
                {
                    switch (t.Status)
                    {
                        case TaskStatus.RanToCompletion:
                            foreach (ChanPost reply in op.replyList)
                            {
                                if (reply.ext != "")
                                {
                                    StackPanel postStackPanel = new StackPanel();
                                    postStackPanel.Orientation = Orientation.Horizontal;
                                    postStackPanel.HorizontalAlignment = HorizontalAlignment.Left;
                                    postStackPanel.Margin = new Thickness(2);
                                    postStackPanel.Background = Brushes.MidnightBlue;
                                    
                                    Image image = new Image();
                                    image.Source = new BitmapImage(new Uri(reply.imageUrl));
                                    image.Stretch = Stretch.None;
                                    image.VerticalAlignment = VerticalAlignment.Top;
                                    image.Margin = new Thickness(2);
                                    postStackPanel.Children.Add(image);

                                    StackPanel textStackPanel = new StackPanel();
                                    textStackPanel.Orientation = Orientation.Vertical;
                                    textStackPanel.VerticalAlignment = VerticalAlignment.Top;

                                    {
                                        TextBlock textBlock = new TextBlock();
                                        Global.htmlToTextBlockText(textBlock,
                                            (reply.no != 0 ? reply.no + " - " : "") + 
                                            (reply.name != "" ? reply.name : "Anonymous") + " @ " +
                                            (reply.now != "" ? reply.now : "UNKNOWN TIME"));
                                        textBlock.Foreground = Brushes.White;
                                        textBlock.TextWrapping = TextWrapping.Wrap;
                                        textBlock.Loaded += (ls, le) =>
                                        {
                                            textBlock.Width = ThreadStackPanel.ActualWidth - image.ActualWidth;
                                        };
                                        textStackPanel.Children.Add(textBlock);
                                    }

                                    {
                                        TextBlock textBlock = new TextBlock();
                                        Global.htmlToTextBlockText(textBlock, "<strong>" + reply.sub + "</strong>");
                                        textBlock.Foreground = Brushes.White;
                                        textBlock.TextWrapping = TextWrapping.Wrap;
                                        textBlock.Loaded += (ls, le) =>
                                        {
                                            textBlock.Width = ThreadStackPanel.ActualWidth - image.ActualWidth;
                                        };
                                        textStackPanel.Children.Add(textBlock);
                                    }

                                    {
                                        TextBlock textBlock = new TextBlock();
                                        Global.htmlToTextBlockText(textBlock, reply.com);
                                        textBlock.Foreground = Brushes.White;
                                        textBlock.TextWrapping = TextWrapping.Wrap;
                                        textBlock.Loaded += (ls, le) => 
                                        {
                                            textBlock.Width = ThreadStackPanel.ActualWidth - image.ActualWidth;
                                        };
                                        textStackPanel.Children.Add(textBlock);
                                    }
                                    
                                    postStackPanel.Children.Add(textStackPanel);

                                    ThreadStackPanel.Children.Add(postStackPanel);
                                }
                            }



                            //foreach (var item in BoardGrid.Children.OfType<StackPanel>()
                            //        .Zip(op.replyList, (i, p) => new { ChanPanel = i, Post = p }))
                            //{
                            //    BitmapImage bitmapImage = new BitmapImage(new Uri(item.Post.imageUrl));
                            //    item.ChanPanel.Children.OfType<Image>().First().Source = bitmapImage;
                            //    item.ChanPanel.Children.OfType<Image>().First().MaxWidth = bitmapImage.Width;
                            //    item.ChanPanel.Children.OfType<Image>().First().MaxHeight = bitmapImage.Height;
                                
                            //    Global.htmlToTextBlockText(item.ChanPanel.Children.OfType<TextBlock>().First(),
                            //         "<strong>" + System.Net.WebUtility.HtmlDecode(item.Post.sub) + "</strong>");
                            //    Global.htmlToTextBlockText(item.ChanPanel.Children.OfType<TextBlock>().Last(),
                            //        System.Net.WebUtility.HtmlDecode(item.Post.com));
                            //}
                            break;
                        case TaskStatus.Canceled:
                            MessageBox.Show("loadThread was canceled!", "CANCELED");
                            break;
                        case TaskStatus.Faulted:
                            MessageBox.Show("loadThread was faulted!", "EXCEPTION");
                            break;
                        default:
                            break;
                    }
                }, TaskScheduler.FromCurrentSynchronizationContext());

        }
    }
}
