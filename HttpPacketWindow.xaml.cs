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

        static HttpPacketInfo currentPacket;

        static int number = 1;
        HashSet<int> ids = new HashSet<int>();
        HashSet<string> domains = new HashSet<string>();

        public HttpPacketWindow()
        {
            this.ContentRendered += (sender, e) => init();
            InitializeComponent();
        }

        private void init()
        {
            number = 1;
            ids = new HashSet<int>();
            string ip = HttpPacketHelper.proxyIp();
            int port = HttpPacketClient.listenPort;
            this.ipLabel.Content = "IP: " + ip;
            StringBuilder tip = new StringBuilder();
            HttpPacketHelper.hostIps().ForEach(str => tip.AppendLine(str));
            this.ipLabel.ToolTip = tip.ToString();
            this.portLabel.Content = "Port: " + port;
            httpDataGrid.DataContext = null;
            domains = MainWindow.domainWhiteList();
            httpDataGrid.DataContext = new ObservableCollection<HttpPacketInfo>();
            ConfigDialogData.httpPacketClient.Start(httpPacketHandler, null);
        }

        private void httpPacketHandler(HttpPacketInfo info)
        {
            if (ids.Contains(info.Id) || !info.show())
            {
                return;
            }
            info.Number = number++;
            ThreadPool.QueueUserWorkItem(o =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (domainFilter.IsChecked == true && !domains.Contains(info.Hostname))
                    {
                        return;
                    }
                    if (domains.Contains(info.Hostname))
                    {
                        info.Pool = MainWindow.queryPoolByUri(info.Uri);
                    }
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
            currentPacket = info;
            drawing();
        }

        private void drawing()
        {
            drawingInfoReq();
            drawingInfoResp();
        }

        private void drawingInfoReq()
        {
            string ut = currentPacket.ut();
            List<string> paramList = currentPacket.queryParams();
            StringBuilder sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine(currentPacket.ReqMethod + " " + currentPacket.FullUrl);
            if (paramList != null && paramList.Count > 0)
            {
                sb.AppendLine().AppendLine("Params：");
                foreach (var item in paramList)
                {
                    sb.AppendLine(item);
                }
            }
            if (!StringHelper.isEmpty(currentPacket.ReqBody))
            {
                sb.AppendLine().AppendLine("Body：");
                sb.AppendLine(currentPacket.ReqBody);
            }
            if (!StringHelper.isEmpty(ut))
            {
                sb.AppendLine().AppendLine("Ut：").AppendLine(ut);
            }
            Dictionary<string, string> headers = currentPacket.ReqHeaders;
            Dictionary<string, List<string>> cookies = currentPacket.ReqCookies;
            if (headers != null && headers.Count > 0)
            {
                sb.AppendLine().AppendLine("Header：");
                foreach (var key in headers.Keys)
                {
                    sb.AppendLine("  " + key + ": " + headers[key]);
                }
            }
            if (cookies != null && cookies.Count > 0)
            {
                sb.AppendLine().AppendLine("Cookies：");
                foreach (var key in cookies.Keys)
                {
                    foreach (var value in cookies[key])
                    {
                        sb.AppendLine("  " + key + "=" + value);
                    }
                }
            }
            this.infoRequestText.Text = sb.ToString();
        }

        private void drawingInfoResp()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine().AppendLine(currentPacket.Response);
            string trace = currentPacket.trace();
            if (trace != null)
            {
                sb.AppendLine().AppendLine("Trace：");
                sb.AppendLine(trace);
            }
            Dictionary<string, string> headers = currentPacket.RespHeaders;
            Dictionary<string, List<string>> cookies = currentPacket.RespCookies;
            if (headers != null && headers.Count > 0)
            {
                sb.AppendLine().AppendLine("Header：");
                foreach (var key in headers.Keys)
                {
                    sb.AppendLine("  " + key + ": " + headers[key]);
                }
            }
            if (cookies != null && cookies.Count > 0)
            {
                sb.AppendLine().AppendLine("Cookies：");
                foreach (var key in cookies.Keys)
                {
                    foreach (var value in cookies[key])
                    {
                        sb.AppendLine("  " + key + "=" + value);
                    }
                }
            }
            this.infoResponseText.Text = sb.ToString();
            string json = null;
            if (currentPacket.respIsJson())
            {
                json = StringHelper.jsonFormat(currentPacket.Response);
            }
            if (json != null)
            {
                this.infoResponseJson.Text = json;
            }
            else
            {
                this.infoResponseJson.Text = "The response is not json:\r\n" + currentPacket.Response;
            }
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
                e.Row.Foreground = new SolidColorBrush(Colors.Gray);
            }
            else if (info.ResponseCode == 500)
            {
                e.Row.Foreground = new SolidColorBrush(Colors.Red);
            }
        }

        private void Clear_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            number = 1;
            ids.Clear();
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
            number = 1;
            ids.Clear();
            ConfigDialogData.httpPacketClient.Shutdown();
            (httpDataGrid.DataContext as ObservableCollection<HttpPacketInfo>).Clear();
        }

    }
}
