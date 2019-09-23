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
    class ResourceHelper
    {

        public static BitmapImage img_doc = getImage("doc.png");
        public static BitmapImage img_del = getImage("del.png");
        public static BitmapImage img_open = getImage("open.png");
        public static BitmapImage img_close = getImage("close.png");
        public static BitmapImage img_not_apply = getImage("not_apply.png");
        public static BitmapImage img_can_apply = getImage("can_apply.png");
        public static BitmapImage img_close_env = getImage("close_env.png");

        public static Color switchColor = (Color)ColorConverter.ConvertFromString("#979DA7");


        public static BitmapImage getImage(string fileName)
        {
            return new BitmapImage(new Uri("pack://siteoforigin:,,,/bin/images/" + fileName));
        }

    }
}
