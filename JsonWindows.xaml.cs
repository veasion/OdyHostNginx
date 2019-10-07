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
    /// Json Format
    /// </summary>
    public partial class JsonWindows : Window
    {
        public JsonWindows()
        {
            InitializeComponent();
        }

        private void FormatBut_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            format();
        }

        private void format()
        {
            string str = this.jsonText.Text;
            string json = StringHelper.jsonFormat(str);
            if (json != null)
            {
                this.jsonText.Text = json;
                this.checkLabel.Content = "json √";
                this.checkLabel.Foreground = new SolidColorBrush(Colors.Green);
                this.formatBut.Source = OdyResources.img_not_apply;
            }
            else
            {
                this.checkLabel.Content = "json ×";
                this.checkLabel.Foreground = new SolidColorBrush(Colors.Red);
            }
        }

        private void JsonText_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            showLine();
        }

        private void JsonText_TextChanged(object sender, TextChangedEventArgs e)
        {
            showLine();
            this.checkLabel.Content = "format =>";
            this.checkLabel.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E5B203"));
            this.formatBut.Source = OdyResources.img_can_apply;
        }

        private void showLine()
        {
            int line = this.jsonText.GetLineIndexFromCharacterIndex(this.jsonText.CaretIndex) + 1;
            this.lineLabel.Content = "line:  " + line;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DataObject.AddPastingHandler(this.jsonText, (arg1, arg2) =>
            {
                ThreadPool.QueueUserWorkItem(o =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Thread.Sleep(200);
                        format();
                    });
                });
            });
        }

    }
}
