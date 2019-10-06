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
using System.Windows.Shapes;

namespace OdyHostNginx
{
    /// <summary>
    /// JsonWindows.xaml 的交互逻辑
    /// </summary>
    public partial class JsonWindows : Window
    {
        public JsonWindows()
        {
            InitializeComponent();
        }

        private void FormatBut_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            string str = this.jsonText.Text;
            string json = StringHelper.jsonFormat(str);
            if (json != null)
            {
                this.jsonText.Text = json;
                this.checkLabel.Content = "json √";
                this.checkLabel.Foreground = new SolidColorBrush(Colors.Green);
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
        }

        private void showLine()
        {
            int line = this.jsonText.GetLineIndexFromCharacterIndex(this.jsonText.CaretIndex) + 1;
            this.lineLabel.Content = "line:  " + line;
        }

    }
}
