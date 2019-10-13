using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OdyHostNginx
{
    class Logger
    {

        public static void info(string log)
        {
            Console.WriteLine(log);
        }

        public static void error(string msg)
        {
            Console.WriteLine(msg);
        }

        public static void error(Exception e)
        {
            if (e != null)
            {
                Console.WriteLine("发生错误：" + e.Message);
            }
        }

        public static void error(string title, Exception e)
        {
            Console.WriteLine(title + "，发生错误" + (e != null ? ("：" + e.Message) : "！"));
        }

    }
}
