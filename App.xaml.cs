using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace OdyHostNginx
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        System.Threading.Mutex mutex;

        public App()
        {
            this.Startup += new StartupEventHandler(AppStartup);
        }

        void AppStartup(object sender, StartupEventArgs e)
        {
            mutex = new System.Threading.Mutex(true, "ElectronicNeedleTherapySystem", out bool ret);
            if (!ret)
            {
                MessageBox.Show("已有一个程序实例运行！", "提示");
                Environment.Exit(0);
            }
            // UI线程异常处理
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
            // 非UI线程异常处理
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            // task线程异常处理
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            Exception ex = e.Exception;
            Logger.error("全局监听，未处理UI线程异常", ex);
            e.Handled = true;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = e.ExceptionObject as Exception;
            Logger.error("全局监听，未处理非UI线程异常", ex);
        }

        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            Exception ex = e.Exception;
            Logger.error("全局监听，未处理task线程异常", ex);
        }
    }
}
