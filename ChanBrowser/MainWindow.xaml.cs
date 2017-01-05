﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
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
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;
using System.Globalization;

namespace ChanBrowser
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        System.Threading.CancellationTokenSource tokenSource = new System.Threading.CancellationTokenSource();
        public static ObservableCollection<ChanPost> chanThreadList = new ObservableCollection<ChanPost>();

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
            BoardListBox.DataContext = chanThreadList;
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
                            toolTip.Placement = System.Windows.Controls.Primitives.PlacementMode.Right;
                            ToolTipService.SetToolTip(boardButton, toolTip);
                        }
                        break;
                    case TaskStatus.Canceled:
                        MessageBox.Show("getBoardList was canceled!", "CANCELED");
                        break;
                    case TaskStatus.Faulted:
                        MessageBox.Show("getBoardList was faulted!", "EXCEPTION");
                        System.Diagnostics.Debugger.Break();
                        break;
                }
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        public async static Task loadBoard(string board, CancellationToken cancellationToken = new CancellationToken())
        {
            chanThreadList.Clear();
            Global.currentBoard = board;

            string address = Global.BASE_URL + board + "/catalog.json";
            JArray boardData = (JArray)await HttpRequest.httpRequestParse(address, JArray.Parse);

            foreach (JObject boardPage in boardData)
            {
                foreach (JObject jsonThread in boardPage["threads"])
                {
                    chanThreadList.Add(new ChanPost(jsonThread, board));
                }
            }

            //chanThreadList.Sort((ChanPost a, ChanPost b) => { return (a.sticky == b.sticky) ? b.last_modified - a.last_modified : b.sticky - a.sticky; });
        }

        private void BoardButton_Click(object sender, RoutedEventArgs e)
        {
            chanThreadList.Clear();

            Tuple<string, string, string> boardInfo = ((Tuple<string, string, string>)((FrameworkElement)sender).DataContext);
            Global.currentBoard = boardInfo.Item1;
            Title = boardInfo.Item1 + " - " + boardInfo.Item2;
            Task<object> task = HttpRequest.httpRequestParse(Global.BASE_URL + ((Button)sender).Content.ToString() + "/catalog.json", JArray.Parse);
            task.ContinueWith( t =>
            {
                switch (t.Status)
                {
                    case TaskStatus.RanToCompletion:
                        foreach (var boardPage in (JArray)t.Result)
                        {
                            foreach (JObject jsonThread in boardPage["threads"])
                            {
                                chanThreadList.Add(new ChanPost(jsonThread, ((Button)sender).Content.ToString()));
                            }
                        }
                        chanThreadList.OrderBy(chanPost => chanPost.last_modified);
                        break;
                    case TaskStatus.Canceled:
                        break;
                    case TaskStatus.Faulted:
                        break;
                    default:
                        break;
                }
            }, TaskScheduler.FromCurrentSynchronizationContext());
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

                ThreadButton_Click(sender, null, stackPanel);
            }
        }
        private void ThreadButton_Click(object sender, RoutedEventArgs e)
        {
            ThreadButton_Click(sender, e, null);
        }
        private async void ThreadButton_Click(object sender, RoutedEventArgs e, StackPanel threadSidebarStackPanel)
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
                                    ToolTipService.SetInitialShowDelay(image, 0);
                                    ToolTipService.SetPlacement(image, System.Windows.Controls.Primitives.PlacementMode.Absolute);
                                    imageToolTip.HorizontalOffset = 100;
                                    imageToolTip.VerticalOffset = 200;

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
                            if (t.Exception.InnerException.Message == "404-NotFound")
                            {
                                if (sender.GetType() == typeof(Grid))
                                {
                                    ((Grid)sender).Background = Brushes.DarkRed;
                                    ((Grid)sender).MouseLeftButtonUp -= ThreadPanel_MouseUp;
                                    if (threadSidebarStackPanel != null)
                                    {
                                        ThreadList.Children.Remove(threadSidebarStackPanel);
                                    }
                                }
                                else if (sender.GetType() == typeof(Button))
                                {
                                    ((StackPanel)((Button)sender).Parent).Background = Brushes.DarkRed;
                                    ((StackPanel)((Button)sender).Parent).Children.OfType<CheckBox>().First().IsChecked = false;
                                    ((StackPanel)((Button)sender).Parent).Children.OfType<CheckBox>().First().IsEnabled = false;
                                }
                                MessageBox.Show("Thread has 404'd!\n");
                            }
                            else
                            {
                                MessageBox.Show("loadThread was faulted!", "EXCEPTION");
                                System.Diagnostics.Debugger.Break();
                            }
                            break;
                        default:
                            break;
                    }
                }, TaskScheduler.FromCurrentSynchronizationContext());

        }

        private void BoardListBox_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta < 0)
            {
                System.Windows.Controls.Primitives.ScrollBar.LineDownCommand.Execute(null, e.OriginalSource as IInputElement);
            }
            if (e.Delta > 0)
            {
                System.Windows.Controls.Primitives.ScrollBar.LineUpCommand.Execute(null, e.OriginalSource as IInputElement);
            }
            e.Handled = true;
        }

        private void TextBlock_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            
        }

        private void Grid_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            
        }

        private void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            TextBlock textBlock = ((Grid)sender).Children.OfType<TextBlock>().Last();
            
            textBlock.Width = BoardListBox.ActualWidth -
                ((ChanPost)textBlock.DataContext).tn_w -
                ((
                    ((Grid)sender).Margin.Left +
                    ((Grid)sender).Margin.Right +
                    ((Grid)sender).Children.OfType<Image>().First().Margin.Left +
                    ((Grid)sender).Children.OfType<Image>().First().Margin.Right +
                    textBlock.Margin.Left + textBlock.Margin.Right
                ) * 3);
        }
    }

    public class CommentWidthConverter : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (double)value / 2;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
