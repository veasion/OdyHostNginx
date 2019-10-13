using System;
using System.Collections.Generic;
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
    /// Trace
    /// </summary>
    public partial class TraceWindows : Window
    {

        public TraceWindows()
        {
            ThreadPool.SetMaxThreads(100, 10);
            InitializeComponent();
        }

        private void LoadBut_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            string url = this.traceUrlText.Text.Trim();
            if (StringHelper.isBlank(url))
            {
                MessageBox.Show("URL不能为空！", "提示");
                return;
            }
            if (url.StartsWith("http://") || url.StartsWith("https://"))
            {
                drawingTrace(url);
            }
            else
            {
                MessageBox.Show("URL格式错误！", "提示");
            }
        }

        private void drawingTrace(string url)
        {
            this.traceTreeInfoText.Text = "";
            this.traceScroll.ScrollToTop();
            this.traceScroll.ScrollToLeftEnd();
            this.traceDataGrid.Visibility = Visibility.Hidden;
            this.traceTreeGroup.Visibility = Visibility.Visible;
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

        private void TraceDataGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.traceScroll.ScrollToLeftEnd();
        }

        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            OdyEvents.ScrollViewer_PreviewMouseWheel(sender, e);
        }

    }
}
