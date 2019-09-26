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
        List<HostConfig> userHosts;
        List<HostConfig> hostConfigs;
        OdyProjectConfig odyProjectConfig;
        Dictionary<string, UpstreamDetails> upstreamDetailsMap;
        Dictionary<string, Image> envSwitchUI = new Dictionary<string, Image>();
        Dictionary<string, EnvConfig> envMap = new Dictionary<string, EnvConfig>();

        StackPanel configViewer = new StackPanel
        {
            Orientation = Orientation.Vertical
        };

        object hostViewer = null;

        public MainWindow()
        {
            this.ContentRendered += (sender, e) => initData();
            InitializeComponent();
        }

        private void initData()
        {
            upstreamDetailsMap = new Dictionary<string, UpstreamDetails>();
            odyProjectConfig = OdyConfigHelper.loadConfig(null, upstreamDetailsMap);
            if (odyProjectConfig == null || odyProjectConfig.Projects == null || odyProjectConfig.Projects.Count == 0)
            {
                odyProjectConfig = ApplicationHelper.copyUserConfigToNginx(true);
            }
            userHosts = OdyConfigHelper.loadUserHosts();
            hostConfigs = OdyConfigHelper.getHosts(odyProjectConfig);
            drawingSwitchUI();
        }

        private void apply()
        {
            ApplicationHelper.applyNginx(odyProjectConfig);
            hostConfigs = OdyConfigHelper.getHosts(odyProjectConfig);
            Dictionary<string, HostConfig> hostDic = new Dictionary<string, HostConfig>();
            if (hostConfigs != null)
            {
                hostConfigs.ForEach(host => hostDic[host.Domain] = host);
            }
            if (userHosts != null)
            {
                userHosts.ForEach(host => hostDic[host.Domain] = host);
            }
            ApplicationHelper.applySwitch(new List<HostConfig>(hostDic.Values));
            this.applyBut.Source = global::OdyHostNginx.Resources.img_not_apply;
        }

        /// <summary>
        /// 总开关
        /// </summary>
        private void OdyHostNginxBut_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Image img = (Image)sender;
            odyProjectConfig.Use = !odyProjectConfig.Use;
            apply();
            img.Source = odyProjectConfig.Use ? global::OdyHostNginx.Resources.img_open : global::OdyHostNginx.Resources.img_close;
            img.ToolTip = odyProjectConfig.Use ? "Close OdyHostNginx" : "Open OdyHostNginx";
            EnvConfig env;
            foreach (var key in envMap.Keys)
            {
                env = envMap[key];
                if (env != null && env.Use)
                {
                    Image image;
                    envSwitchUI.TryGetValue(key, out image);
                    if (image != null)
                    {
                        image.Source = odyProjectConfig.Use ? global::OdyHostNginx.Resources.img_open : global::OdyHostNginx.Resources.img_open_disable;
                    }
                }
            }
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
            apply();
        }

        private void ResetBut_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (isHostConfig)
            {
                userHosts = OdyConfigHelper.loadUserHosts();
                hostConfigs = OdyConfigHelper.getHosts(odyProjectConfig);
            }
            else if (currentEnv != null)
            {
                OdyConfigHelper.reloadEnv(currentEnv, false);
            }
            drawingEnvConfig(currentEnv, true);
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

        private void Button_Config_Host_Click(object sender, RoutedEventArgs e)
        {
            Button but = (Button)sender;
            if (but != null && but.Content != null && "Host".Equals(but.Content.ToString().Trim()))
            {
                // host click
                isHostConfig = true;
                this.resetBut.ToolTip = "reset host";
                this.hostBut.Background = new SolidColorBrush(global::OdyHostNginx.Resources.butClickColor);
                this.configBut.Background = new SolidColorBrush(global::OdyHostNginx.Resources.butInitColor);
                this.configHostViewer.Content = hostViewer;
                // drawingHostConfig();
            }
            else
            {
                // config click
                isHostConfig = false;
                this.resetBut.ToolTip = "reset config";
                this.configBut.Background = new SolidColorBrush(global::OdyHostNginx.Resources.butClickColor);
                this.hostBut.Background = new SolidColorBrush(global::OdyHostNginx.Resources.butInitColor);
                this.configHostViewer.Content = configViewer;
                // drawingNginxConfig();
            }
        }

        /// <summary>
        /// 绘制 Switch UI
        /// </summary>
        private void drawingSwitchUI()
        {
            this.ProjectSwitch.Children.Clear();
            EnvConfig firstEnv = null;
            foreach (var project in odyProjectConfig.Projects)
            {
                int envCount = project.Envs != null ? project.Envs.Count : 0;
                Border border = new Border
                {
                    BorderThickness = new Thickness(0, 1, 0, 0),
                    BorderBrush = new SolidColorBrush(global::OdyHostNginx.Resources.switchBorderColor)
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
                    Foreground = new SolidColorBrush(global::OdyHostNginx.Resources.switchColor)
                };

                // project name
                uniformGrid.Children.Add(label);

                // envs
                foreach (var env in project.Envs)
                {
                    if (firstEnv == null)
                    {
                        firstEnv = env;
                    }

                    string key = envKey(env);

                    DockPanel dockRoot = new DockPanel
                    {
                        Height = 45,
                        DataContext = key,
                        Cursor = Cursors.Hand
                    };

                    dockRoot.MouseLeftButtonUp += EnvDockRoot_MouseLeftButtonUp;

                    // doc image
                    DockPanel dockLeftImg = new DockPanel
                    {
                        Width = 50
                    };
                    DockPanel.SetDock(dockLeftImg, Dock.Left);
                    Image docImg = new Image
                    {
                        Width = 16,
                        Source = global::OdyHostNginx.Resources.img_doc
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
                        Source = global::OdyHostNginx.Resources.img_close,
                        Margin = new Thickness(0, 6, 0, 0)
                    };
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
                        Foreground = new SolidColorBrush(global::OdyHostNginx.Resources.switchColor)
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
            drawingEnvConfig(firstEnv);
        }

        private void EnvDockRoot_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            DockPanel dock = (DockPanel)sender;
            string key = (string)dock.DataContext;
            if (key != null)
            {
                EnvConfig env;
                envMap.TryGetValue(key, out env);
                if (env != null)
                {
                    drawingEnvConfig(env);
                }
            }
        }

        private void EnvSwitchMouseButtonEventHandler(object sender, MouseButtonEventArgs e)
        {
            Image envSwitchImg = (Image)sender;
            string key = (string)envSwitchImg.DataContext;
            EnvConfig env;
            envMap.TryGetValue(key, out env);
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
                            Image eui;
                            envSwitchUI.TryGetValue(envKey(envConfig), out eui);
                            if (eui != null)
                            {
                                eui.Source = global::OdyHostNginx.Resources.img_close;
                            }
                        }
                    }
                }
                envSwitchImg.Source = env.Use ? global::OdyHostNginx.Resources.img_open : global::OdyHostNginx.Resources.img_close;
                apply();
            }
        }

        private string envKey(EnvConfig env)
        {
            return env.Project.Name + env.EnvName;
        }

        private EnvConfig getFirstEnv()
        {
            foreach (var p in odyProjectConfig.Projects)
            {
                foreach (var env in p.Envs)
                {
                    return env;
                }
            }
            return null;
        }

        private void drawingEnvConfig(EnvConfig env)
        {
            drawingEnvConfig(env, false);
        }

        /// <summary>
        /// 绘制 env config
        /// </summary>
        private void drawingEnvConfig(EnvConfig env, bool force)
        {
            if (!force && env != null && currentEnv == env)
            {
                return;
            }
            if (currentEnv != null)
            {
                changeEnv(currentEnv, false);
            }
            currentEnv = env;
            if (currentEnv == null)
            {
                currentEnv = getFirstEnv();
            }
            if (isHostConfig)
            {
                this.hostBut.Background = new SolidColorBrush(global::OdyHostNginx.Resources.butClickColor);
                this.configBut.Background = new SolidColorBrush(global::OdyHostNginx.Resources.butInitColor);
            }
            else
            {
                this.configBut.Background = new SolidColorBrush(global::OdyHostNginx.Resources.butClickColor);
                this.hostBut.Background = new SolidColorBrush(global::OdyHostNginx.Resources.butInitColor);
            }
            // env switch dock color
            changeEnv(currentEnv, true);
            // drawing nginx config
            drawingNginxConfig();
            // drawing host config
            drawingHostConfig();
            if (isHostConfig)
            {
                // host
                this.configHostViewer.Content = hostViewer;
            }
            else
            {
                // config
                this.configHostViewer.Content = configViewer;
            }
        }

        private void drawingNginxConfig()
        {
            // 渲染 nginx config
            configViewer.Children.Clear();
            OdyConfigHelper.sortUpstream(currentEnv.Upstreams);
            foreach (var u in currentEnv.Upstreams)
            {
                UpstreamDetails ud;
                upstreamDetailsMap.TryGetValue(u.ServerName, out ud);
                HashSet<string> contextPaths = new HashSet<string>();
                if (ud != null)
                {
                    ud.ContextPaths.ForEach(name => contextPaths.Add(name));
                }
                if (contextPaths.Count == 0)
                {
                    contextPaths.Add(u.ServerName);
                }
                foreach (var contextPath in contextPaths)
                {
                    Border border = new Border
                    {
                        Margin = new Thickness(5, 10, 5, 0),
                        CornerRadius = new CornerRadius(22),
                        BorderThickness = new Thickness(1, 1, 1, 1),
                        BorderBrush = new SolidColorBrush(global::OdyHostNginx.Resources.configBorderColor)
                    };
                    DockPanel dockRoot = new DockPanel
                    {
                        Height = 45
                    };

                    // Server Name
                    DockPanel dockServer = new DockPanel
                    {
                        Width = 200
                    };
                    DockPanel.SetDock(dockServer, Dock.Left);
                    StringBuilder tip = new StringBuilder();
                    tip.Append("=== ");
                    tip.Append(contextPath);
                    tip.Append(" ===");
                    if (ud != null)
                    {
                        tip.AppendLine();
                        ud.Uris.ForEach(uri => tip.AppendLine(uri));
                    }
                    Label serverNameLabel = new Label
                    {
                        FontSize = 14,
                        Content = contextPath,
                        ToolTip = tip.ToString(),
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Foreground = new SolidColorBrush(global::OdyHostNginx.Resources.configFontColor)
                    };
                    dockServer.Children.Add(serverNameLabel);

                    // IP
                    DockPanel dockIp = new DockPanel
                    {
                        Width = 130
                    };
                    DockPanel.SetDock(dockIp, Dock.Left);
                    TextBox ipText = new TextBox
                    {
                        Text = u.Ip,
                        FontSize = 12,
                        BorderThickness = new Thickness(1, 0, 0, 0),
                        VerticalContentAlignment = VerticalAlignment.Center,
                        HorizontalContentAlignment = HorizontalAlignment.Center,
                        Background = new SolidColorBrush(global::OdyHostNginx.Resources.configBgColor),
                        Foreground = new SolidColorBrush(global::OdyHostNginx.Resources.configFontColor)
                    };
                    ipText.DataContext = u;
                    ipText.TextChanged += IpText_TextChanged;
                    dockIp.Children.Add(ipText);

                    // Port
                    DockPanel dockPort = new DockPanel
                    {
                        Width = 80
                    };
                    DockPanel.SetDock(dockPort, Dock.Left);
                    TextBox portText = new TextBox
                    {
                        FontSize = 12,
                        Text = u.Port + "",
                        BorderThickness = new Thickness(1, 0, 1, 0),
                        VerticalContentAlignment = VerticalAlignment.Center,
                        HorizontalContentAlignment = HorizontalAlignment.Center,
                        Background = new SolidColorBrush(global::OdyHostNginx.Resources.configBgColor),
                        Foreground = new SolidColorBrush(global::OdyHostNginx.Resources.configFontColor)
                    };
                    portText.DataContext = u;
                    portText.KeyDown += PortText_KeyDown;
                    portText.TextChanged += PortText_TextChanged;
                    portText.PreviewTextInput += PortText_PreviewTextInput;
                    InputMethod.SetIsInputMethodEnabled(portText, false);
                    dockPort.Children.Add(portText);

                    // local button
                    DockPanel dockLocal = new DockPanel
                    {
                        Width = 80
                    };
                    DockPanel.SetDock(dockPort, Dock.Left);
                    Button localBut = new Button
                    {
                        Width = 42,
                        Height = 28,
                        Content = "local",
                        Cursor = Cursors.Hand,
                        ToolTip = "Set to local IP",
                        Background = new SolidColorBrush(global::OdyHostNginx.Resources.butInitColor),
                        Foreground = new SolidColorBrush(global::OdyHostNginx.Resources.configFontColor)
                    };
                    localBut.DataContext = u;
                    localBut.Click += LocalBut_Click;
                    dockLocal.Children.Add(localBut);

                    dockRoot.Children.Add(dockServer);
                    dockRoot.Children.Add(dockIp);
                    dockRoot.Children.Add(dockPort);
                    dockRoot.Children.Add(dockLocal);

                    border.Child = dockRoot;
                    configViewer.Children.Add(border);
                }
            }
        }

        private void LocalBut_Click(object sender, RoutedEventArgs e)
        {
            Button but = sender as Button;
            NginxUpstream u = (NginxUpstream)but.DataContext;
            if ("local".Equals(but.Content))
            {
                u.Ip = "127.0.0.1";
                but.Content = "back";
            }
            else
            {
                u.Ip = u.OldIp;
                u.Port = u.OldPort;
                but.Content = "local";
            }
            DockPanel dockRoot = (DockPanel)VisualTreeHelper.GetParent(VisualTreeHelper.GetParent(but));
            ((dockRoot.Children[1] as DockPanel).Children[0] as TextBox).Text = u.Ip;
            ((dockRoot.Children[2] as DockPanel).Children[0] as TextBox).Text = u.Port + "";
            this.applyBut.Source = global::OdyHostNginx.Resources.img_can_apply;
        }

        private void PortText_KeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = (sender as TextBox).SelectionStart > 5;
        }

        private void PortText_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !StringHelper.isInt(e.Text);
        }

        private void PortText_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox port = (TextBox)sender;
            NginxUpstream u = (NginxUpstream)port.DataContext;
            if (StringHelper.isInt(port.Text.Trim()))
            {
                u.Port = Convert.ToInt32(port.Text.Trim());
                this.applyBut.Source = global::OdyHostNginx.Resources.img_can_apply;
            }
            port.Foreground = new SolidColorBrush(StringHelper.isPort(port.Text) ? global::OdyHostNginx.Resources.configFontColor : Colors.Red);
        }

        private void IpText_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox ip = (TextBox)sender;
            NginxUpstream u = (NginxUpstream)ip.DataContext;
            u.Ip = ip.Text;
            bool isIp = StringHelper.isIp(u.Ip);
            if (isIp)
            {
                this.applyBut.Source = global::OdyHostNginx.Resources.img_can_apply;
            }
            ip.Foreground = new SolidColorBrush(isIp ? global::OdyHostNginx.Resources.configFontColor : Colors.Red);
        }

        private void drawingHostConfig()
        {
            // 渲染 host config
            // TODO hostViewer
        }

        private void changeEnv(EnvConfig env, bool isCurrent)
        {
            if (env == null)
            {
                return;
            }
            string key = envKey(env);
            Image switchImg;
            envSwitchUI.TryGetValue(key, out switchImg);
            if (switchImg != null)
            {
                DockPanel dockRoot = (DockPanel)VisualTreeHelper.GetParent(VisualTreeHelper.GetParent(switchImg));
                if (isCurrent)
                {
                    dockRoot.Background = new SolidColorBrush(global::OdyHostNginx.Resources.switchCurrentBackgroundColor);
                }
                else
                {
                    dockRoot.Background = new SolidColorBrush(global::OdyHostNginx.Resources.switchBackgroundColor);
                }
            }
        }

    }
}
