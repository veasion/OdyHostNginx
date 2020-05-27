using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace OdyHostNginx
{
    class ExtHelper
    {

        const string extPath = "ext.cf";

        public static void loadExtTools(MenuItem toolsMenu)
        {
            for (int i = toolsMenu.Items.Count - 1; i >= 0; i--)
            {
                MenuItem item = toolsMenu.Items.GetItemAt(i) as MenuItem;
                if (item != null && item.DataContext != null && item.DataContext.Equals(true))
                {
                    toolsMenu.Items.RemoveAt(i);
                }
            }
            if (!File.Exists(extPath)) return;
            List<MenuItem> list = new List<MenuItem>();
            FileHelper.readTextFile(extPath, Encoding.UTF8, (line, text) =>
            {
                text = text.Trim();
                if (!StringHelper.isBlank(text) && !text.StartsWith("#"))
                {
                    MenuItem item = createItem(text);
                    if (item != null)
                    {
                        list.Add(item);
                    }
                }
                return true;
            });
            if (list.Count > 0)
            {
                Logger.info("正在加载扩展菜单...");
                toolsMenu.Items.Add(createItem("{\"name\":\"---\"}"));
            }
            list.ForEach(o => toolsMenu.Items.Add(o));
        }

        private static MenuItem createItem(string json)
        {
            try
            {
                JToken jt = JToken.Parse(json);
                MenuItem item = new MenuItem();
                string name = jt["name"].ToString();
                string run = null;
                if (jt["run"] != null)
                {
                    run = jt["run"].ToString();
                }
                item.Header = name;
                item.FontSize = 12;
                item.DataContext = true;
                item.Background = new SolidColorBrush(Colors.White);
                item.Foreground = new SolidColorBrush(Colors.Black);
                if (run != null)
                {
                    item.Click += (sender, e) => {
                        CmdHelper.Cmd(run);
                    };
                }
                return item;
            }
            catch (Exception e) {
                Logger.error("加载扩展菜单异常", e);
            }
            return null;
        }
    }
}
