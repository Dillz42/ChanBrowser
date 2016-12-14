using System;
using System.Collections.Generic;
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
        public MainWindow()
        {
            InitializeComponent();

            Task<List<string>> boardListTask = Global.getBoardList();
            boardListTask.ContinueWith(t =>
            {
                switch (t.Status)
                {
                    case TaskStatus.RanToCompletion:
                        foreach (string board in t.Result)
                        {
                            Button boardButton = new Button();
                            boardButton.MinWidth = 100;
                            boardButton.Margin = new Thickness(1);
                            boardButton.Content = "/" + board + "/";
                            boardButton.Click += BoardButton_Click;
                            BoardList.Children.Add(boardButton);
                        }
                        break;
                    case TaskStatus.Canceled:
                        MessageBox.Show("getBoardList was canceled!");
                        break;
                    case TaskStatus.Faulted:
                        MessageBox.Show("getBoardList was faulted!");
                        break;
                    default:
                        break;
                }
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }


        private async void BoardButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Board was clicked!");
        }
    }
}
