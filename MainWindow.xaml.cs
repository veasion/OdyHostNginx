using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OdyHostNginx
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {

        bool isHostConfig;
        EnvConfig currentEnv;
        List<HostConfig> hostConfigs;
        OdyProjectConfig odyProjectConfig;
        Dictionary<string, Image> envSwitchUI = new Dictionary<string, Image>();
        Dictionary<string, EnvConfig> envMap = new Dictionary<string, EnvConfig>();

        public MainWindow()
        {
            InitializeComponent();
            odyProjectConfig = OdyConfigHelper.loadConfig(null);
            if (odyProjectConfig == null || odyProjectConfig.Projects == null || odyProjectConfig.Projects.Count == 0)
            {
                odyProjectConfig = ApplicationHelper.copyUserConfigToNginx(true);
            }
            hostConfigs = OdyConfigHelper.getHosts(odyProjectConfig);
            drawingSwitchUI();
        }

        /// <summary>
        /// 总开关
        /// </summary>
        private void OdyHostNginxBut_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Image img = (Image)sender;
            odyProjectConfig.Use = !odyProjectConfig.Use;
            img.Source = odyProjectConfig.Use ? ResourceHelper.img_open : ResourceHelper.img_close;
            img.ToolTip = odyProjectConfig.Use ? "Close OdyHostNginx" : "Open OdyHostNginx";
            apply();
        }

        private void Reload_Click(object sender, RoutedEventArgs e)
        {
            odyProjectConfig = ApplicationHelper.copyUserConfigToNginx(false);
            drawingSwitchUI();
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            odyProjectConfig = ApplicationHelper.copyUserConfigToNginx(true);
            drawingSwitchUI();
        }

        private void ApplyBut_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Image img = (Image)sender;
            img.Source = ResourceHelper.img_not_apply;
            apply();
        }

        private void ResetBut_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // TODO currentEnv reset
            drawingCurrentEnvUI();
            apply();
        }

        private void OdyHostNginx_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("您确定要退出吗？", "提示", MessageBoxButton.OKCancel, MessageBoxImage.None, MessageBoxResult.Cancel);
            if (result == MessageBoxResult.Cancel)
            {
                e.Cancel = true;
            }
            else
            {
                ApplicationHelper.exit();
            }
        }

        private void apply()
        {
            // ApplicationHelper.applyNginx(odyProjectConfig);
            // ApplicationHelper.applySwitch(hostConfigs);
        }

        /// <summary>
        /// 绘制 env config UI
        /// </summary>
        private void drawingCurrentEnvUI()
        {
            // currentEnv
            // 选中样式
            // 配置渲染, isHostConfig ? host : config

        }

        /// <summary>
        /// 绘制 Switch UI
        /// </summary>
        private void drawingSwitchUI()
        {
            this.ProjectSwitch.Children.Clear();
            foreach (var project in odyProjectConfig.Projects)
            {
                int envCount = project.Envs != null ? project.Envs.Count : 0;
                Border border = new Border
                {
                    BorderThickness = new Thickness(0, 1, 0, 0),
                    BorderBrush = new SolidColorBrush(ResourceHelper.switchColor)
                };
                UniformGrid uniformGrid = new UniformGrid
                {
                    Rows = envCount + 1
                };
                Label label = new Label
                {
                    Height = 30,
                    FontSize = 16,
                    Content = project.Name,
                    FontWeight = FontWeights.Bold,
                    Margin = new Thickness(6, 0, 0, 0),
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Foreground = new SolidColorBrush(ResourceHelper.switchColor)
                };

                // project name
                uniformGrid.Children.Add(label);

                // envs
                foreach (var env in project.Envs)
                {
                    if (currentEnv == null)
                    {
                        currentEnv = env;
                    }
                    DockPanel dockRoot = new DockPanel
                    {
                        Height = 45
                    };

                    // doc image
                    DockPanel dockLeftImg = new DockPanel
                    {
                        Width = 50
                    };
                    DockPanel.SetDock(dockLeftImg, Dock.Left);
                    Image docImg = new Image
                    {
                        Width = 16,
                        Source = ResourceHelper.img_doc
                    };
                    dockLeftImg.Children.Add(docImg);

                    // close image
                    DockPanel dockRightImg = new DockPanel();
                    dockLeftImg.Width = 80;
                    DockPanel.SetDock(dockRightImg, Dock.Right);
                    Image envSwitchImg = new Image
                    {
                        Width = 80,
                        Cursor = Cursors.Hand,
                        Source = ResourceHelper.img_close,
                        Margin = new Thickness(0, 6, 0, 0)
                    };
                    string key = envKey(env);
                    envSwitchImg.DataContext = key;
                    envSwitchImg.MouseLeftButtonUp += EnvSwitchMouseButtonEventHandler;
                    dockRightImg.Children.Add(envSwitchImg);

                    envMap[key] = env;
                    envSwitchUI[key] = envSwitchImg;

                    // env
                    DockPanel dockLeftLabel = new DockPanel();
                    DockPanel.SetDock(dockLeftLabel, Dock.Left);
                    Label envLabel = new Label
                    {
                        FontSize = 14,
                        Content = env.EnvName,
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Foreground = new SolidColorBrush(ResourceHelper.switchColor)
                    };
                    dockLeftLabel.Children.Add(envLabel);
                    dockRoot.Children.Add(dockLeftImg);
                    dockRoot.Children.Add(dockRightImg);
                    dockRoot.Children.Add(dockLeftLabel);
                    uniformGrid.Children.Add(dockRoot);
                }
                border.Child = uniformGrid;
                this.ProjectSwitch.Children.Add(border);
            }
            // 绘制 env
            drawingCurrentEnvUI();
        }

        private void EnvSwitchMouseButtonEventHandler(object sender, MouseButtonEventArgs e)
        {
            Image envSwitchImg = (Image)sender;
            string key = (string)envSwitchImg.DataContext;
            EnvConfig env = envMap[key];
            if (env != null && odyProjectConfig.Use)
            {
                env.Use = !env.Use;
                if (env.Use)
                {
                    foreach (var envConfig in env.Project.Envs)
                    {
                        if (envConfig != env)
                        {
                            envConfig.Use = false;
                            Image eui = envSwitchUI[envKey(envConfig)];
                            if (eui != null)
                            {
                                eui.Source = ResourceHelper.img_close;
                            }
                        }
                    }
                }
                envSwitchImg.Source = env.Use ? ResourceHelper.img_open : ResourceHelper.img_close;
                apply();
            }
        }

        private string envKey(EnvConfig env)
        {
            return env.Project.Name + env.EnvName;
        }

        /// <summary>
        /// 绘制 nginx config
        /// </summary>
        private void drawingNginxConfig(EnvConfig env)
        {

        }

    }
}
