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
            Task<List<Tuple<string, string, string>>> boardListTask = Global.getBoardList(tokenSource.Token);
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
                            boardButton.DataContext = board;
                            BoardList.Children.Add(boardButton);

                            ToolTip toolTip = new ToolTip();
                            toolTip.Content = board.Item2 + "\n" + board.Item3;
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
            ((ScrollViewer)BoardStackPanel.Parent).ScrollToTop();
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
                BoardStackPanel.Children.Clear();

                Task task = Global.loadBoard(((Button)sender).Content.ToString(), tokenSource.Token);
                await task.ContinueWith(t =>
                 {
                     switch (t.Status)
                     {
                         case TaskStatus.RanToCompletion:
                             Title = "/" + ((Tuple<string, string, string>)((Button)sender).DataContext).Item1 + "/ - " +
                             ((Tuple<string, string, string>)((Button)sender).DataContext).Item2;

                             foreach (ChanPost chanThread in Global.chanThreadList)
                             {
                                 Grid threadGrid = new Grid();
                                 threadGrid.Margin = new Thickness(3);
                                 threadGrid.DataContext = chanThread;
                                 threadGrid.IsHitTestVisible = true;
                                 threadGrid.MouseLeftButtonUp += ThreadPanel_MouseUp;

                                 threadGrid.RowDefinitions.Add(new RowDefinition());
                                 threadGrid.RowDefinitions.Add(new RowDefinition());
                                 threadGrid.RowDefinitions.Add(new RowDefinition());
                                 threadGrid.RowDefinitions.Add(new RowDefinition());

                                 threadGrid.ColumnDefinitions.Add(new ColumnDefinition());
                                 threadGrid.ColumnDefinitions.Add(new ColumnDefinition());

                                 {
                                     Separator separator = new Separator();
                                     Grid.SetColumn(separator, 0);
                                     Grid.SetRow(separator, 0);
                                     Grid.SetColumnSpan(separator, 2);
                                     threadGrid.Children.Add(separator);
                                 }

                                 //Add meta-data and subject
                                 TextBlock subjectTextBlock = new TextBlock();
                                 Grid.SetColumn(subjectTextBlock, 0);
                                 Grid.SetRow(subjectTextBlock, 1);
                                 Grid.SetColumnSpan(subjectTextBlock, 2);
                                 subjectTextBlock.Foreground = Brushes.White;
                                 subjectTextBlock.TextWrapping = TextWrapping.Wrap;
                                 Global.htmlToTextBlockText(subjectTextBlock,
                                     "R:" + chanThread.replies + "/I:" + chanThread.images +
                                     (chanThread.sub == "" ? "" : " - " + "<strong>" + System.Net.WebUtility.HtmlDecode(chanThread.sub) + "</strong>"));
                                 threadGrid.Children.Add(subjectTextBlock);

                                 //Add image
                                 Image postImage = new Image();
                                 Grid.SetColumn(postImage, 0);
                                 Grid.SetRow(postImage, 2);
                                 postImage.VerticalAlignment = VerticalAlignment.Top;
                                 BitmapImage bitmapImage = new BitmapImage(new Uri(chanThread.imageUrl));
                                 bitmapImage.DownloadCompleted += (ds, de) =>
                                 {
                                     postImage.MaxHeight = bitmapImage.PixelHeight;
                                     postImage.MaxWidth = bitmapImage.PixelWidth;
                                     threadGrid.ColumnDefinitions[0].MaxWidth = bitmapImage.PixelHeight;
                                 };
                                 postImage.Source = bitmapImage;
                                 threadGrid.Children.Add(postImage);

                                 //Add comment
                                 TextBlock commentTextBlock = new TextBlock();
                                 Grid.SetColumn(commentTextBlock, 1);
                                 Grid.SetRow(commentTextBlock, 2);
                                 commentTextBlock.Foreground = Brushes.White;
                                 commentTextBlock.Margin = new Thickness(3);
                                 commentTextBlock.TextWrapping = TextWrapping.Wrap;
                                 Global.htmlToTextBlockText(commentTextBlock, System.Net.WebUtility.HtmlDecode(chanThread.com));
                                 threadGrid.Children.Add(commentTextBlock);

                                 {
                                     Separator separator = new Separator();
                                     Grid.SetColumn(separator, 0);
                                     Grid.SetRow(separator, 3);
                                     Grid.SetColumnSpan(separator, 2);
                                     threadGrid.Children.Add(separator);
                                 }

                                 BoardStackPanel.Children.Add(threadGrid);
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
            if (((FrameworkElement)sender).DataContext != null)
            {
                ChanPost chanPost = ((ChanPost)((FrameworkElement)sender).DataContext);

                StackPanel stackPanel = new StackPanel();
                stackPanel.Orientation = Orientation.Vertical;
                stackPanel.Margin = new Thickness(2);
                stackPanel.Background = Brushes.DarkGreen;

                Button threadButton = new Button();
                threadButton.MinWidth = 100;
                threadButton.Margin = new Thickness(2);
                threadButton.Content = chanPost.no.ToString() + "\n" + chanPost.semantic_url;
                threadButton.DataContext = ((FrameworkElement)sender).DataContext;
                threadButton.Click += ThreadButton_Click;
                stackPanel.Children.Add(threadButton);

                CheckBox checkBox = new CheckBox();
                checkBox.Content = "Auto-Refresh";
                checkBox.IsChecked = false;
                checkBox.Margin = new Thickness(2);
                stackPanel.Children.Add(checkBox);

                Button removeThreadButton = new Button();
                removeThreadButton.Content = "Remove";
                removeThreadButton.Margin = new Thickness(2);
                removeThreadButton.Click += (cs, ce) => { ThreadList.Children.Remove(stackPanel); };
                stackPanel.Children.Add(removeThreadButton);

                ThreadList.Children.Add(stackPanel);

                ThreadButton_Click(threadButton, null);
            }
        }

        private async void ThreadButton_Click(object sender, RoutedEventArgs e)
        {
            ChanPost op = ((ChanPost)((FrameworkElement)sender).DataContext);

            ThreadStackPanel.Children.Clear();
            ((ScrollViewer)ThreadStackPanel.Parent).ScrollToTop();

            Task loadThread = Global.loadThread(op, tokenSource.Token);
            await loadThread.ContinueWith(t =>
                {
                    switch (t.Status)
                    {
                        case TaskStatus.RanToCompletion:
                            foreach (ChanPost reply in op.replyList)
                            {
                                StackPanel postStackPanel = new StackPanel();
                                postStackPanel.Orientation = Orientation.Horizontal;
                                postStackPanel.HorizontalAlignment = HorizontalAlignment.Left;
                                postStackPanel.Margin = new Thickness(2);
                                postStackPanel.Background = Brushes.MidnightBlue;

                                Image image = new Image();

                                if (reply.ext != "")
                                {
                                    image.Source = new BitmapImage(new Uri(reply.imageUrl));
                                    image.Stretch = Stretch.None;
                                    image.VerticalAlignment = VerticalAlignment.Top;
                                    image.Margin = new Thickness(2);
                                    postStackPanel.Children.Add(image);

                                    ToolTip imageToolTip = new ToolTip();
                                    StackPanel toolTipStackPanel = new StackPanel();
                                    TextBlock toolTipTextBlock = new TextBlock();
                                    Image toolTipImage = new Image();

                                    imageToolTip.Background = Brushes.Black;
                                    imageToolTip.Loaded += (ls, le) =>
                                    {
                                        toolTipImage.Source = new BitmapImage(new Uri((string)toolTipImage.DataContext));
                                        toolTipImage.MaxHeight = SystemParameters.PrimaryScreenHeight;
                                        toolTipImage.MaxWidth = SystemParameters.PrimaryScreenHeight;
                                    };

                                    toolTipStackPanel.Orientation = Orientation.Vertical;

                                    toolTipTextBlock.Text = reply.w + "x" + reply.h + " - " + reply.filename + reply.ext;
                                    toolTipStackPanel.Children.Add(toolTipTextBlock);

                                    toolTipImage.DataContext = Global.BASE_IMAGE_URL + reply.board + "/" + reply.tim + reply.ext;
                                    toolTipStackPanel.Children.Add(toolTipImage);

                                    imageToolTip.Content = toolTipStackPanel;
                                    ToolTipService.SetShowDuration(image, int.MaxValue);
                                    //ToolTipService.SetInitialShowDelay(image, 0);
                                    ToolTipService.SetPlacement(image, System.Windows.Controls.Primitives.PlacementMode.Absolute);

                                    ToolTipService.SetToolTip(image, imageToolTip);
                                }

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
                                        textBlock.Width = ThreadStackPanel.ActualWidth - image.ActualWidth - SystemParameters.VerticalScrollBarWidth;
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
                                        textBlock.Width = ThreadStackPanel.ActualWidth - image.ActualWidth - SystemParameters.VerticalScrollBarWidth;
                                    };
                                    textStackPanel.Children.Add(textBlock);
                                }

                                postStackPanel.Children.Add(textStackPanel);

                                ThreadStackPanel.Children.Add(postStackPanel);
                            }

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
