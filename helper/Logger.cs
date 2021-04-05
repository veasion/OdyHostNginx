using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace OdyHostNginx
{
    public class Logger
    {

        public static LoggerHandler logHandler;

        public delegate void LoggerHandler(string log);

        public static void info(string log)
        {
            printLog(log);
        }

        public static void error(string msg)
        {
            printLog(msg);
        }

        public static void error(Exception e)
        {
            error(null, e);
        }

        public static void error(string title, Exception e)
        {
            if (title != null)
            {
                printLog(title + "，发生错误" + (e != null ? ("：" + e.Message) : "！"));
            }
            else if (e != null)
            {
                printLog("发生错误：" + e.Message);
            }
            if (e != null)
            {
                printLog(e.StackTrace);
            }
        }

        private static void printLog(string log)
        {
            Console.WriteLine(log);
            if (logHandler != null)
            {
                ThreadPool.QueueUserWorkItem(o =>
                {
                    if (logHandler != null)
                    {
                        new LoggerHandler(logHandler)(log);
                    }
                });
            }
        }

    }
}
