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
    /// Format
    /// </summary>
    public partial class FormatWindows : Window
    {

        private string type;
        private Formatter formatter;

        public FormatWindows()
        {
            type = "json";
            formatter = FormatterFactory.GetFormatter(type);
            InitializeComponent();
        }

        private void FormatBut_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            format();
        }

        private void format()
        {
            string text = this.formatText.Text;
            string format = formatter.Format(text);
            if (format != null)
            {
                this.formatText.Text = format;
                this.checkLabel.Content = "Complete";
                this.checkLabel.Foreground = new SolidColorBrush(Colors.Green);
                this.formatBut.Source = OdyResources.img_not_apply;
            }
            else
            {
                this.checkLabel.Content = type + " ×";
                this.checkLabel.Foreground = new SolidColorBrush(Colors.Red);
            }
        }

        private void Format_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            showLine();
        }

        private void Format_TextChanged(object sender, TextChangedEventArgs e)
        {
            showLine();
            this.checkLabel.Content = "format =>";
            this.checkLabel.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E5B203"));
            this.formatBut.Source = OdyResources.img_can_apply;
        }

        private void showLine()
        {
            int line = this.formatText.GetLineIndexFromCharacterIndex(this.formatText.CaretIndex) + 1;
            this.lineLabel.Content = "line:  " + line;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DataObject.AddPastingHandler(this.formatText, (arg1, arg2) =>
            {
                ThreadPool.QueueUserWorkItem(o =>
                {
                    Thread.Sleep(200);
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        format();
                    });
                });
            });
        }

        private void RadioButton_Click(object sender, RoutedEventArgs e)
        {
            RadioButton radio = sender as RadioButton;
            type = radio.Content as string;
            if (radio.DataContext != null)
            {
                this.Title = radio.DataContext as string;
            }
            else
            {
                this.Title = type + " format";
            }
            this.groupBox.Header = type;
            formatter = FormatterFactory.GetFormatter(type);
            format();
        }

    }
}
