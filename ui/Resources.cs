using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace OdyHostNginx
{
    class Resources
    {

        public static BitmapImage img_doc = getImage("doc.png");
        public static BitmapImage img_del = getImage("del.png");
        public static BitmapImage img_open = getImage("open.png");
        public static BitmapImage img_close = getImage("close.png");
        public static BitmapImage img_not_apply = getImage("not_apply.png");
        public static BitmapImage img_can_apply = getImage("can_apply.png");
        public static BitmapImage img_close_env = getImage("close_env.png");
        public static BitmapImage img_open_disable = getImage("open_disable.png");

        public static Color butClickColor = Colors.White;
        public static Color configBgColor = Colors.White;
        public static Color switchColor = (Color)ColorConverter.ConvertFromString("#979DA7");
        public static Color butInitColor = (Color)ColorConverter.ConvertFromString("#FFF0F0F0");
        public static Color configFontColor = (Color)ColorConverter.ConvertFromString("#979DA7");
        public static Color configBorderColor = (Color)ColorConverter.ConvertFromString("#979DA7");
        public static Color switchBorderColor = (Color)ColorConverter.ConvertFromString("#FF2D3138");
        public static Color switchBackgroundColor = (Color)ColorConverter.ConvertFromString("#FF373D47");
        public static Color switchCurrentBackgroundColor = (Color)ColorConverter.ConvertFromString("#FF2D3138");


        public static BitmapImage getImage(string fileName)
        {
            return new BitmapImage(new Uri("pack://siteoforigin:,,,/bin/images/" + fileName));
        }

    }
}
