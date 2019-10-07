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

        static int number = 1;

        bool currentTraceTab = false;
        HttpPacketInfo currentPacket;
        HashSet<int> ids = new HashSet<int>();
        HashSet<string> domains = new HashSet<string>();

        public HttpPacketWindow()
        {
            ThreadPool.SetMaxThreads(100, 10);
            this.ContentRendered += (sender, e) => init();
            InitializeComponent();
        }

        #region packet
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
                    bool isOdy = domains.Contains(info.Hostname) || info.Hostname.EndsWith("oudianyun.com");
                    if (domainFilter.IsChecked == true && !isOdy)
                    {
                        return;
                    }
                    if (isOdy)
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
        #endregion

        #region request/response
        private void drawing()
        {
            if (currentTraceTab)
            {
                drawingTrace();
            }
            drawingInfoReq();
            drawingInfoResp();
        }

        private void drawingInfoReq()
        {
            if (currentPacket == null)
            {
                this.infoRequestText.Text = "";
                return;
            }
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
            if (currentPacket == null)
            {
                this.infoResponseText.Text = "";
                this.infoResponseJson.Text = "";
                return;
            }
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
        #endregion

        #region trace
        private void drawingTrace()
        {
            this.traceTreeInfoText.Text = "";
            this.traceTreeGroup.Visibility = Visibility.Hidden;
            this.traceDataGrid.Visibility = Visibility.Hidden;
            if (currentPacket == null) return;
            string url = currentPacket.trace();
            ThreadPool.QueueUserWorkItem(o =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (StringHelper.isEmpty(url)) return;
                    TracesInfo trace = TraceClient.traces(url);
                    if (trace == null) return;
                    TreeView tree = new TreeView();
                    tree.BorderThickness = new Thickness(0);
                    tree.Margin = new Thickness(10, 10, 10, 10);
                    tree.SelectedItemChanged += Trace_SelectedItemChanged;
                    tree.Background = new SolidColorBrush(OdyResources.butInitColor);
                    tree.CommandBindings.Add(new CommandBinding(ApplicationCommands.Copy));
                    traceTreeChildren(tree, trace);
                    this.traceTreeGroup.Content = tree;
                    this.traceTreeGroup.Visibility = Visibility.Visible;
                });
            });
        }

        private void Trace_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            this.traceScroll.ScrollToTop();
            this.traceScroll.ScrollToHome();
            ItemsControl item = e.NewValue as ItemsControl;
            if (item != null)
            {
                TracesInfo trace = item.DataContext as TracesInfo;
                if (trace != null)
                {
                    drawingTraceDetails(trace);
                    StringBuilder sb = new StringBuilder();
                    sb.Append("\tid: ").Append(trace.Id);
                    if (trace.Timestamp != null && trace.Timestamp > 0)
                    {
                        sb.Append("\tdate: ").Append(StringHelper.timeStampFormat(trace.Timestamp.Value / 1000, null));
                    }
                    if (trace.Duration != null)
                    {
                        sb.Append("\ttime: ").Append(trace.Duration.Value / 1000).Append("ms");
                    }
                    if (trace.ServiceName != null)
                    {
                        sb.Append("\tservice: ").Append(trace.ServiceName);
                    }
                    if (trace.ClientName != null)
                    {
                        sb.Append("\tclient: ").Append(trace.ClientName);
                    }
                    if (trace.Name != null)
                    {
                        sb.Append("\tname: ").Append(trace.Name);
                    }
                    this.traceTreeInfoText.Text = sb.ToString();
                }
            }
        }

        private void traceTreeChildren(ItemsControl parent, TracesInfo trace)
        {
            if (trace.Name != null && trace.Name.EndsWith("companyid")) return;
            TreeViewItem item = new TreeViewItem();
            item.DataContext = trace;
            item.Header = "【" + trace.Pool() + "】" + trace.Name;
            parent.Items.Add(item);
            if (trace.Children != null)
            {
                foreach (var t in trace.Children)
                {
                    traceTreeChildren(item, t);
                }
            }
        }

        private void drawingTraceDetails(TracesInfo trace)
        {
            this.traceDataGrid.Visibility = Visibility.Visible;
            DataGrid traceDataGrid = this.traceDataGrid;
            traceDataGrid.DataContext = KeyValue.list(trace.Details);
        }
        #endregion

        #region other
        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems != null && e.AddedItems.Count > 0)
            {
                TabItem item = e.AddedItems[0] as TabItem;
                if (item != null && "Trace".Equals(item.Header))
                {
                    currentTraceTab = true;
                    drawingTrace();
                }
                else
                {
                    currentTraceTab = false;
                }
            }
        }

        private void HttpDataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            HttpPacketInfo info = e.Row.Item as HttpPacketInfo;
            if (info == null) return;
            e.Row.ToolTip = info.FullUrl;
            if (info.ResponseCode == 404)
            {
                e.Row.Foreground = new SolidColorBrush(Colors.Gray);
            }
            else if (info.isError())
            {
                e.Row.Foreground = new SolidColorBrush(Colors.Red);
            }
            else if (info.ResponseCode == 200)
            {
                e.Row.Foreground = new SolidColorBrush(Colors.Black);
            }
            else
            {
                e.Row.Foreground = new SolidColorBrush(Colors.Gray);
            }
        }

        private void HttpPacketSwitchBut_Click(object sender, RoutedEventArgs e)
        {
            CheckBox check = (CheckBox)sender;
            bool isChecked = check.IsChecked != null && check.IsChecked == true ? true : false;
            if (isChecked)
            {
                init();
                check.ToolTip = "Close Http Packet";
            }
            else
            {
                number = 1;
                ids.Clear();
                currentPacket = null;
                check.ToolTip = "Open Http Packet";
                ConfigDialogData.httpPacketClient.Shutdown();
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
            delImage.Source = OdyResources.img_del_red;
        }

        private void DelImage_MouseLeave(object sender, MouseEventArgs e)
        {
            Image delImage = sender as Image;
            delImage.Source = OdyResources.img_del_grey;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            number = 1;
            ids.Clear();
            MainWindow.HttpPacketWindowClose();
            ConfigDialogData.httpPacketClient.Shutdown();
            (httpDataGrid.DataContext as ObservableCollection<HttpPacketInfo>).Clear();
        }
        #endregion

    }
}
