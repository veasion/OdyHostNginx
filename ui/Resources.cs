using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace OdyHostNginx
{
    public class Resources
    {

        public static BitmapImage img_doc = getImage("doc.png");
        public static BitmapImage img_del_red = getImage("del_red.png");
        public static BitmapImage img_del_grey = getImage("del_grey.png");
        public static BitmapImage img_add_grey = getImage("add_grey.png");
        public static BitmapImage img_add_blue = getImage("add_blue.png");
        public static BitmapImage img_not_apply = getImage("not_apply.png");
        public static BitmapImage img_can_apply = getImage("can_apply.png");

        public static Color hostBgColor = Colors.White;
        public static Color butClickColor = Colors.White;
        public static Color configBgColor = Colors.White;
        public static Color selectBorderColor = Colors.LightBlue;

        public static Color switchColor = (Color)ColorConverter.ConvertFromString("#979DA7");
        public static Color hostFontColor = (Color)ColorConverter.ConvertFromString("#979DA7");
        public static Color butInitColor = (Color)ColorConverter.ConvertFromString("#FFF0F0F0");
        public static Color configFontColor = (Color)ColorConverter.ConvertFromString("#979DA7");
        public static Color configBorderColor = (Color)ColorConverter.ConvertFromString("#979DA7");
        public static Color switchBorderColor = (Color)ColorConverter.ConvertFromString("#FF2D3138");
        public static Color switchBackgroundColor = (Color)ColorConverter.ConvertFromString("#FF373D47");
        public static Color switchCurrentBackgroundColor = (Color)ColorConverter.ConvertFromString("#FF2D3138");

        public static Style switchButStyle = getResource("ui/SwitchButDictionary.xaml", "SliderCheckBox") as Style;

        public static BitmapImage getImage(string fileName)
        {
            return new BitmapImage(new Uri("pack://siteoforigin:,,,/bin/images/" + fileName));
        }

        public static object getResource(string uri, string key)
        {
            return ((ResourceDictionary)Application.LoadComponent(new Uri(uri, UriKind.Relative)))[key];
        }

    }
}
