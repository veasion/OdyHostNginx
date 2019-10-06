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
    /// TraceWindows.xaml 的交互逻辑
    /// </summary>
    public partial class TraceWindows : Window
    {

        public TraceWindows()
        {
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
            this.traceTreeGroup.Visibility = Visibility.Hidden;
            this.traceDataGrid.Visibility = Visibility.Hidden;
            if (StringHelper.isEmpty(url)) return;
            TracesInfo trace = TraceClient.traces(url);
            if (trace == null)
            {
                MessageBox.Show("加载失败！", "提示");
                return;
            }
            TreeView tree = new TreeView();
            tree.SelectedItemChanged += Trace_SelectedItemChanged;
            tree.Margin = new Thickness(10, 10, 10, 10);
            tree.BorderThickness = new Thickness(0);
            tree.Background = new SolidColorBrush(OdyResources.butInitColor);
            traceTreeChildren(tree, trace);
            this.traceTreeGroup.Content = tree;
            this.traceTreeGroup.Visibility = Visibility.Visible;
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
                    this.traceTreeInfoText.Text = sb.ToString();
                }
            }
        }

        private void traceTreeChildren(ItemsControl parent, TracesInfo trace)
        {
            if ("handlegetcompanyid".Equals(trace.Name) || "handleputcompanyid".Equals(trace.Name)) return;
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

    }
}
