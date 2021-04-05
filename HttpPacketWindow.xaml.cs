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

        string span = "    ";
        static bool https = false;
        bool currentTraceTab = false;
        HttpPacketInfo currentPacket;
        ModifyRequstWindows modifyRequstWindows;
        HashSet<string> ids = new HashSet<string>();
        HashSet<string> domains = new HashSet<string>();
        List<ModifyRequstBean> modifys = new List<ModifyRequstBean>();
        ObservableCollection<HttpPacketInfo> list = new ObservableCollection<HttpPacketInfo>();

        public HttpPacketWindow()
        {
            ThreadPool.SetMaxThreads(100, 10);
            this.ContentRendered += (sender, e) => init();
            InitializeComponent();
        }

        #region packet
        private void init()
        {
            clear();
            int port = HttpPacketClient.listenPort;
            StringBuilder tip = new StringBuilder();
            HttpPacketHelper.hostIps().ForEach(str => tip.AppendLine(str));
            this.computerImage.ToolTip = tip.ToString();
            this.portLabel.Content = "Port: " + port;
            httpDataGrid.DataContext = null;
            domains = MainWindow.domainWhiteList();
            https = this.httpsFilter.IsChecked == true;
            try
            {
                HttpPacketClient.InstallCertificate();
            }
            catch (Exception e)
            {
                Logger.error("安装证书", e);
            }
            // open packet client
            ConfigDialogData.httpPacketClient.Start(httpPacketHandler, modifys, https, null);
        }

        private void clear()
        {
            Monitor.Enter(list);
            number = 1;
            ids.Clear();
            list.Clear();
            httpDataGrid.DataContext = null;
            Monitor.Exit(list);
        }

        private void httpPacketHandler(HttpPacketInfo info)
        {
            ThreadPool.QueueUserWorkItem(o =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (ids.Contains(info.Id) || !info.show(https, modifyResponse.IsChecked == true))
                    {
                        return;
                    }
                    info.Number = number++;
                    bool isOdy = domains.Contains(info.Hostname) || info.Hostname.EndsWith("oudianyun.com");
                    if (domainFilter.IsChecked == true && !isOdy)
                    {
                        return;
                    }
                    if (isOdy)
                    {
                        info.Pool = MainWindow.queryPoolByUri(info.Uri);
                    }
                    ids.Add(info.Id);
                    if (list.Count > 200)
                    {
                        clear();
                    }
                    list.Add(info);
                    httpDataGrid.DataContext = filter(this.searchText.Text);
                });
            });
        }

        private ObservableCollection<HttpPacketInfo> filter(string query)
        {
            if (query == null || StringHelper.isBlank(query = query.Trim()))
            {
                return list;
            }
            ObservableCollection<HttpPacketInfo> result = new ObservableCollection<HttpPacketInfo>();
            foreach (var item in list)
            {
                if (item.FullUrl.Contains(query))
                {
                    result.Add(item);
                }
                else if (item.ReqBody != null && item.ReqBody.Contains(query))
                {
                    result.Add(item);
                }
                else if (item.Response != null && item.Response.Contains(query))
                {
                    result.Add(item);
                }
            }
            return result;
        }

        private void SearchText_KeyUp(object sender, KeyEventArgs e)
        {
            Key k = e.Key;
            bool search = false;
            if (k == Key.Enter)
            {
                search = true;
            }
            else if (StringHelper.isEmpty(this.searchText.Text))
            {
                search = true;
            }
            if (search)
            {
                httpDataGrid.DataContext = filter(this.searchText.Text);
            }
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
                    sb.AppendLine(span + item);
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
                    sb.AppendLine(span + key + ": " + headers[key]);
                }
            }
            if (cookies != null && cookies.Count > 0)
            {
                sb.AppendLine().AppendLine("Cookies：");
                foreach (var key in cookies.Keys)
                {
                    foreach (var value in cookies[key])
                    {
                        sb.AppendLine(span + key + "=" + value);
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
                    sb.AppendLine(span + key + ": " + headers[key]);
                }
            }
            if (cookies != null && cookies.Count > 0)
            {
                sb.AppendLine().AppendLine("Cookies：");
                foreach (var key in cookies.Keys)
                {
                    foreach (var value in cookies[key])
                    {
                        sb.AppendLine(span + key + "=" + value);
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
                if (currentPacket.IsModify)
                {
                    json = "The response is modify:\r\n\r\n" + json;
                }
                this.infoResponseJson.Text = json;
            }
            else if (currentPacket.IsModify)
            {
                this.infoResponseJson.Text = "The response is modify:\r\n\r\n" + currentPacket.Response;
            }
            else
            {
                this.infoResponseJson.Text = "The response is not json:\r\n\r\n" + currentPacket.Response;
            }
        }
        #endregion

        #region trace
        private void drawingTrace()
        {
            this.traceTreeInfoText.Text = "";
            this.traceScroll.ScrollToTop();
            this.traceScroll.ScrollToLeftEnd();
            this.traceTreeGroup.Visibility = Visibility.Hidden;
            this.traceDataGrid.Visibility = Visibility.Hidden;
            if (currentPacket == null) return;
            this.traceTreeGroup.Visibility = Visibility.Visible;
            string url = currentPacket.trace();
            if (StringHelper.isEmpty(url))
            {
                this.traceTreeGroup.Content = new Label
                {
                    Content = "None",
                    VerticalContentAlignment = VerticalAlignment.Center,
                    HorizontalContentAlignment = HorizontalAlignment.Center
                };
                return;
            }
            else
            {
                this.traceTreeGroup.Content = new Image
                {
                    Width = 50,
                    Source = OdyResources.img_load
                };
            }
            ThreadPool.QueueUserWorkItem(o =>
            {
                TracesInfo trace = TraceClient.traces(url);
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (trace == null)
                    {
                        this.traceTreeGroup.Content = new Label
                        {
                            Content = "Load failed !",
                            VerticalContentAlignment = VerticalAlignment.Center,
                            HorizontalContentAlignment = HorizontalAlignment.Center
                        };
                        return;
                    }
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
                    if (trace.Timestamp != null && trace.Timestamp > 0)
                    {
                        sb.Append("date: ").Append(StringHelper.timeStampFormat(trace.Timestamp.Value, null));
                    }
                    if (trace.Duration != null)
                    {
                        sb.Append("\ttime: ").Append(trace.Duration.Value).Append("ms");
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
            if (trace.Pool() != null)
            {
                item.Header = "【" + trace.Pool() + "】" + trace.Name;
            }
            else
            {
                item.Header = trace.Name;
            }
            if (trace.isError())
            {
                item.Foreground = new SolidColorBrush(OdyResources.errorFontColor);
                item.Background = new SolidColorBrush(OdyResources.errorBackgroundColor);
            }
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

        private void TraceDataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            KeyValue kv = e.Row.Item as KeyValue;
            if (kv == null) return;
            if (kv.Key != null && "error".Equals(kv.Key.Trim().ToLower()))
            {
                e.Row.Foreground = new SolidColorBrush(OdyResources.errorFontColor);
                e.Row.Background = new SolidColorBrush(OdyResources.errorBackgroundColor);
            }
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
            if (info.Status == 404 || info.IsModify)
            {
                e.Row.Foreground = new SolidColorBrush(Colors.Gray);
            }
            else if (info.isError())
            {
                e.Row.Foreground = new SolidColorBrush(OdyResources.errorFontColor);
                e.Row.Background = new SolidColorBrush(OdyResources.errorBackgroundColor);
            }
            else if (info.Status == 200)
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

        private void HttpsFilter_Click(object sender, RoutedEventArgs e)
        {
            init();
        }

        private void ModifyResponse_Click(object sender, RoutedEventArgs e)
        {
            ConfigDialogData.modifyResponse = this.modifyResponse.IsChecked == true;
        }

        private void Clear_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            clear();
        }

        private void SearchText_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (StringHelper.isEmpty(this.searchText.Text))
            {
                this.searchText.BorderBrush = new SolidColorBrush(OdyResources.borderColor);
            }
            else
            {
                this.searchText.BorderBrush = new SolidColorBrush(OdyResources.selectBorderColor);
            }
        }

        private void TraceDataGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.traceScroll.ScrollToLeftEnd();
        }

        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            OdyEvents.ScrollViewer_PreviewMouseWheel(sender, e);
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
            clear();
            MainWindow.HttpPacketWindowClose();
            ConfigDialogData.httpPacketClient.Shutdown();
        }
        #endregion

        private void MenuItem_Click_CopyUrl(object sender, RoutedEventArgs e)
        {
            if (currentPacket != null)
            {
                Clipboard.SetText(currentPacket.FullUrl);
            }
        }

        private void MenuItem_Click_ReplayXHR(object sender, RoutedEventArgs e)
        {
            if (currentPacket != null)
            {
                if (!HttpHelper.ReplayXHR(currentPacket))
                {
                    MessageBox.Show("Replay请求失败");
                }
            }
        }

        private void MenuItem_Click_SaveAsText(object sender, RoutedEventArgs e)
        {
            if (currentPacket != null)
            {
                drawingInfoReq();
                drawingInfoResp();
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Request:");
                sb.AppendLine(this.infoRequestText.Text);
                sb.AppendLine();
                sb.AppendLine("Response:");
                sb.AppendLine(this.infoResponseText.Text);
                System.Windows.Forms.SaveFileDialog dialog = new System.Windows.Forms.SaveFileDialog
                {
                    Title = "Save As Text",
                    RestoreDirectory = true,
                    FileName = "response.txt",
                    Filter = "TXT(*.txt)|*.txt"
                };
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    FileHelper.writeFile(dialog.FileName, sb.ToString());
                }
            }
        }

        private void MenuItem_Click_Modify(object sender, RoutedEventArgs e)
        {
            showModifyAllWindows();
        }

        private void MenuItem_Click_Modify_Request(object sender, RoutedEventArgs e)
        {
            showModifyRequstWindows(0);
        }

        private void MenuItem_Click_Modify_Response(object sender, RoutedEventArgs e)
        {
            showModifyRequstWindows(1);
        }

        private void showModifyRequstWindows(int interceptType)
        {
            string title = "Modify Request";
            ModifyRequstBean bean = new ModifyRequstBean();
            bean.InterceptType = interceptType;
            if (currentPacket != null)
            {
                HttpPacketInfo info = (HttpPacketInfo)currentPacket.Clone();
                bean.MatchType = 1;
                bean.MatchText = info.FullUrl;
                if (interceptType == 0)
                {
                    title = "Modify Request - " + info.FullUrl;
                    int index = info.FullUrl.IndexOf("?");
                    if (index != -1)
                    {
                        bean.ParamsStr = info.FullUrl.Substring(index + 1);
                    }
                    else
                    {
                        bean.ParamsStr = "";
                    }
                    bean.Body = info.ReqBody;
                    bean.Headers = new Dictionary<string, string>(info.ReqHeaders);
                }
                else
                {
                    title = "Modify Response - " + info.FullUrl;
                    bean.Body = info.Response;
                    bean.Headers = new Dictionary<string, string>(info.RespHeaders);
                }
            }
            else
            {
                bean.Headers = new Dictionary<string, string>();
                bean.Headers.Add("Content-Type", "application/json;charset=utf-8");
            }
            List<ModifyRequstBean> init = new List<ModifyRequstBean>
            {
                bean
            };
            List<ModifyRequstBean> history = new List<ModifyRequstBean>();
            ModifyRequstWindows windows = new ModifyRequstWindows(init, (list) =>
            {
                if (list != null && list.Count > 0)
                {
                    foreach (var item in history)
                    {
                        if (!list.Contains(item))
                        {
                            modifys.Remove(item);
                        }
                    }
                    history.Clear();
                    foreach (var item in list)
                    {
                        if (!modifys.Contains(item))
                        {
                            modifys.Add(item);
                            history.Add(item);
                        }
                    }
                }
                else
                {
                    foreach (var item in history)
                    {
                        modifys.Remove(item);
                    }
                    history.Clear();
                }
                if (modifyRequstWindows != null && !modifyRequstWindows.IsDisposed)
                {
                    modifyRequstWindows.updateData(modifys);
                }
            });
            windows.Text = title;
            windows.Show();
        }

        private void showModifyAllWindows()
        {
            if (modifyRequstWindows == null || modifyRequstWindows.IsDisposed)
            {
                modifyRequstWindows = new ModifyRequstWindows(modifys, (list) =>
                {
                    modifys.Clear();
                    if (list != null)
                    {
                        modifys.AddRange(list);
                    }
                });
                modifyRequstWindows.Text = "Modify All";
            }
            modifyRequstWindows.Show();
        }
    }
}
