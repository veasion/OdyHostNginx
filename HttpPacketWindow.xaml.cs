using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Shapes;

namespace OdyHostNginx
{
    /// <summary>
    /// HttpPacket
    /// </summary>
    public partial class HttpPacketWindow : Window
    {

        public static bool filterDomain = true;

        public HttpPacketWindow()
        {
            this.ContentRendered += (sender, e) => init();
            InitializeComponent();
        }

        private void init()
        {
            filterDomain = false; // TODO
            string ip = HttpPacketHelper.proxyIp();
            int port = HttpPacketClient.listenPort;
            this.ipLabel.Content = "IP: " + ip;
            StringBuilder tip = new StringBuilder();
            HttpPacketHelper.hostIps().ForEach(str => tip.AppendLine(str));
            this.ipLabel.ToolTip = tip.ToString();
            this.portLabel.Content = "Port: " + port;
            httpDataGrid.DataContext = null;
            HashSet<string> domains = null;
            if (filterDomain)
            {
                domains = MainWindow.domainWhiteList();
            }
            httpDataGrid.DataContext = new ObservableCollection<HttpPacketInfo>();
            ConfigDialogData.httpPacketClient.Start(httpPacketHandler, domains);
        }

        private void httpPacketHandler(HttpPacketInfo info)
        {
            ThreadPool.QueueUserWorkItem(o =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Monitor.Enter(httpDataGrid.DataContext);
                    (httpDataGrid.DataContext as ObservableCollection<HttpPacketInfo>).Add(info);
                    Monitor.Exit(httpDataGrid.DataContext);
                });
            });
        }

        private void HttpDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            HttpPacketInfo info = httpDataGrid.SelectedItem as HttpPacketInfo;
            if (info == null) return;
            Console.WriteLine("选中：id {0}, url {1}", info.Id, info.FullUrl);
        }

        private void HttpDataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            HttpPacketInfo info = e.Row.Item as HttpPacketInfo;
            if (info == null) return;
            e.Row.ToolTip = info.FullUrl;
            if (info.ResponseCode == 200)
            {
                // e.Row.Foreground = new SolidColorBrush(Colors.Green);
            }
            else if (info.ResponseCode == 404)
            {
                e.Row.Foreground = new SolidColorBrush(Colors.White);
            }
            else if (info.ResponseCode == 500)
            {
                e.Row.Foreground = new SolidColorBrush(Colors.Red);
            }
        }

        private void Clear_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            (httpDataGrid.DataContext as ObservableCollection<HttpPacketInfo>).Clear();
        }

        private void DelImage_MouseEnter(object sender, MouseEventArgs e)
        {
            Image delImage = sender as Image;
            delImage.Source = global::OdyHostNginx.Resources.img_del_red;
        }

        private void DelImage_MouseLeave(object sender, MouseEventArgs e)
        {
            Image delImage = sender as Image;
            delImage.Source = global::OdyHostNginx.Resources.img_del_grey;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ConfigDialogData.httpPacketClient.Shutdown();
            (httpDataGrid.DataContext as ObservableCollection<HttpPacketInfo>).Clear();
        }

    }
}
