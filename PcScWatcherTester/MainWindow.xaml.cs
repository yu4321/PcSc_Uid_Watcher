using PcScWatcher;
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

namespace PcScWatcherTester
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        private SCardWatcher watcher = null;
        public MainWindow()
        {
            InitializeComponent();
            Title = "Not Connected";
            watcher = new SCardWatcher();
            watcher.UidReceived += Watcher_UidReceived;
            watcher.LogAction += new Action<string>((s) =>
            {
                Dispatcher.Invoke(() =>
                {
                    tb.Text += $"{DateTime.Now} {s} \n";
                });
            });
            try
            {
                watcher.Initialize();
                if (watcher.IsInitialized)
                {
                    Title = watcher.ConnectedDeviceName;
                }
            }
            catch(Exception e)
            {
                recvdValue.Text = e.Message;
            }
          
           
        }

        private void Watcher_UidReceived(object sender, CardUidReceivedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                recvdValue.Text = BitConverter.ToString(e.ReceivedData);
            });
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                tb.ScrollToEnd();
            });
        }
    }
}
