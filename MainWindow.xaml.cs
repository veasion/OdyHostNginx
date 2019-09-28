using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace OdyHostNginx
{
    /// <summary>
    /// OdyHostNginx
    /// </summary>
    public partial class MainWindow : Window
    {
        #region 变量
        bool isHostConfig;
        string hostSearch;
        string configSearch;
        EnvConfig currentEnv;
        List<HostConfig> userHosts;
        OdyProjectConfig odyProjectConfig;
        Dictionary<string, UpstreamDetails> upstreamDetailsMap;
        Dictionary<string, EnvConfig> envMap = new Dictionary<string, EnvConfig>();
        Dictionary<string, CheckBox> envSwitchUI = new Dictionary<string, CheckBox>();

        StackPanel configViewer = new StackPanel
        {
            Orientation = Orientation.Vertical
        };

        StackPanel hostViewer = new StackPanel
        {
            Orientation = Orientation.Vertical
        };
        #endregion

        #region 初始化、退出
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
            drawingSwitchUI();
        }

        private void apply()
        {
            ApplicationHelper.applyNginx(odyProjectConfig);
            ApplicationHelper.applySwitch(getCurrentHost());
            OdyConfigHelper.writeUserHosts(userHosts);
            this.applyBut.Source = global::OdyHostNginx.Resources.img_not_apply;
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

        private List<HostConfig> getCurrentHost()
        {
            Dictionary<string, HostConfig> hostDic = new Dictionary<string, HostConfig>();
            // 总开关控制所有 host
            if (odyProjectConfig.Use)
            {
                // env 开关控制 env host
                if (currentEnv != null && currentEnv.Use && currentEnv.Hosts != null)
                {
                    foreach (var host in currentEnv.Hosts)
                    {
                        if (host.Use)
                        {
                            hostDic[host.Domain] = host;
                        }
                    }
                }
                // user host 不受 env 开关影响
                if (userHosts != null)
                {
                    foreach (var host in userHosts)
                    {
                        if (host.Use)
                        {
                            hostDic[host.Domain] = host;
                        }
                    }
                }
            }
            return new List<HostConfig>(hostDic.Values);
        }
        #endregion

        #region 菜单栏按钮

        private void Export_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("该配置将导出到桌面，是否继续？", "导出提示", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                string path = FileHelper.getDesktopDirectory() + "\\ExportConfigs";
                OdyConfigHelper.writeConfig(odyProjectConfig, path, true);
                MessageBox.Show("导出成功！\r\n\r\n路径：" + path, "导出提示");
            }
        }

        private void Import_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog
            {
                Multiselect = true,
                Title = "导入nginx配置文件",
                Filter = "(nginx 配置文件 *.conf) | *.conf",
                InitialDirectory = FileHelper.getDesktopDirectory()
            };
            if (ofd.ShowDialog() == true)
            {
                if (ofd.FileNames != null && ofd.FileNames.Length >= 0)
                {
                    new NginxConfigWindows().ShowDialog();
                    if (ConfigDialogData.success && ConfigDialogData.path != null)
                    {
                        FileHelper.copyFiles(ofd.FileNames, ConfigDialogData.path, true);
                        MessageBox.Show("导入成功！");
                        odyProjectConfig = ApplicationHelper.copyUserConfigToNginx(true);
                        initData();
                    }
                }
            }
        }

        private void Explorer_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer.exe", FileHelper.getCurrentDirectory());
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(" 非常感谢使用这个小工具").AppendLine();
            sb.AppendLine(" 该工具专门为欧电云开发小伙伴打造").AppendLine();
            sb.AppendLine(" 业余开发，如有漏洞或建议请在非工作时间钉我~");
            MessageBox.Show(sb.ToString(), "关于", MessageBoxButton.OK, MessageBoxImage.Asterisk);
        }

        private void HostConfig_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer.exe", OdyConfigHelper.userHostsDir);
        }

        private void NginxConfig_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer.exe", OdyConfigHelper.nginxConfigDir);
        }

        private void UserConfig_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer.exe", OdyConfigHelper.userNginxConfigDir);
        }
        
        private void ShowLog_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("notepad.exe", WindowsNginxImpl.nginxLogPath);
        }

        private void ShowHosts_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("====== sys hosts ======");
            FileHelper.readTextFile(WindowsLocalHostImpl.hostsPath, WindowsLocalHostImpl.hostsEncoding, (index, line) =>
            {
                if (!StringHelper.isBlank(line))
                {
                    line = line.Trim();
                    if (!line.StartsWith("#"))
                    {
                        sb.AppendLine(line);
                    }
                }
            });
            MessageBox.Show(sb.ToString(), "hosts");
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

        private void DingTalk_Click(object sender, RoutedEventArgs e)
        {
            CmdHelper.openDingTalk();
        }

        private void Trace_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("功能待开发，敬请期待！");
        }

        private void HttpPacket_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("功能待开发，敬请期待！");
        }
        #endregion

        #region 环境开关
        /// <summary>
        /// 总开关
        /// </summary>
        private void OdyHostNginxBut_Click(object sender, RoutedEventArgs e)
        {
            CheckBox check = (CheckBox)sender;
            odyProjectConfig.Use = check.IsChecked != null && check.IsChecked == true ? true : false;
            apply();
            check.ToolTip = odyProjectConfig.Use ? "Close OdyHostNginx" : "Open OdyHostNginx";
            EnvConfig env;
            foreach (var key in envMap.Keys)
            {
                env = envMap[key];
                if (env != null && env.Use)
                {
                    CheckBox box;
                    envSwitchUI.TryGetValue(key, out box);
                    if (box != null)
                    {
                        box.IsEnabled = odyProjectConfig.Use;
                    }
                }
            }
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

        private void EnvSwitchClickEventHandler(object sender, RoutedEventArgs e)
        {
            CheckBox envSwitch = (CheckBox)sender;
            string key = (string)envSwitch.DataContext;
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
                            CheckBox eui;
                            envSwitchUI.TryGetValue(envKey(envConfig), out eui);
                            if (eui != null && eui.IsChecked == true)
                            {
                                eui.IsChecked = false;
                            }
                        }
                    }
                }
                envSwitch.IsChecked = env.Use;
                apply();
            }
            else if (env != null)
            {
                envSwitch.IsChecked = env.Use;
            }
            drawingEnvConfig(env);
        }
        #endregion

        #region 环境ui渲染
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
                    ToolTip = project.Name,
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
                    uniformGrid.Children.Add(drawingEnvUI(env));
                }
                border.Child = uniformGrid;
                this.ProjectSwitch.Children.Add(border);
            }
            // 绘制 env
            drawingEnvConfig(firstEnv);
        }

        private DockPanel drawingEnvUI(EnvConfig env)
        {
            string key = envKey(env);

            DockPanel dockRoot = new DockPanel
            {
                Height = 45,
                DataContext = key,
                Cursor = Cursors.Hand,
                Background = new SolidColorBrush(global::OdyHostNginx.Resources.switchBackgroundColor)
            };

            dockRoot.MouseLeftButtonUp += EnvDockRoot_MouseLeftButtonUp;

            // doc image
            DockPanel dockLeftDoc = new DockPanel
            {
                Width = 50
            };
            DockPanel.SetDock(dockLeftDoc, Dock.Left);
            Image docImg = new Image
            {
                Width = 16,
                Source = global::OdyHostNginx.Resources.img_doc
            };
            dockLeftDoc.Children.Add(docImg);

            // switch
            DockPanel dockRightSwitch = new DockPanel();
            dockRightSwitch.Width = 80;
            DockPanel.SetDock(dockRightSwitch, Dock.Right);
            CheckBox envSwitch = new CheckBox
            {
                Cursor = Cursors.Hand,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Style = global::OdyHostNginx.Resources.switchButStyle
            };
            envSwitch.DataContext = key;
            envSwitch.Click += EnvSwitchClickEventHandler;
            dockRightSwitch.Children.Add(envSwitch);

            envMap[key] = env;
            envSwitchUI[key] = envSwitch;

            // env
            DockPanel dockLeftLabel = new DockPanel();
            DockPanel.SetDock(dockLeftLabel, Dock.Left);
            Label envLabel = new Label
            {
                FontSize = 14,
                Content = env.EnvName,
                ToolTip = env.EnvName,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left,
                Foreground = new SolidColorBrush(global::OdyHostNginx.Resources.switchColor)
            };
            dockLeftLabel.Children.Add(envLabel);
            dockRoot.Children.Add(dockLeftDoc);
            dockRoot.Children.Add(dockRightSwitch);
            dockRoot.Children.Add(dockLeftLabel);
            return dockRoot;
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
        #endregion

        #region config/host switch
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
        }

        private void Button_Config_Host_Click(object sender, RoutedEventArgs e)
        {
            Button but = (Button)sender;
            if (but != null && but.Content != null && "Host".Equals(but.Content.ToString().Trim()))
            {
                if (isHostConfig)
                {
                    return;
                }
                // host click
                isHostConfig = true;
                this.resetBut.ToolTip = "reset host";
                this.addBut.Visibility = Visibility.Visible;
                this.hostBut.Background = new SolidColorBrush(global::OdyHostNginx.Resources.butClickColor);
                this.configBut.Background = new SolidColorBrush(global::OdyHostNginx.Resources.butInitColor);
                if (hostViewer.Children.Count == 0 && StringHelper.isBlank(hostSearch))
                {
                    drawingHostConfig();
                }
                this.configHostViewer.ScrollToTop();
                this.configHostViewer.Content = hostViewer;
            }
            else
            {
                if (!isHostConfig)
                {
                    return;
                }
                // config click
                isHostConfig = false;
                this.resetBut.ToolTip = "reset config";
                this.addBut.Visibility = Visibility.Hidden;
                this.configBut.Background = new SolidColorBrush(global::OdyHostNginx.Resources.butClickColor);
                this.hostBut.Background = new SolidColorBrush(global::OdyHostNginx.Resources.butInitColor);
                if (configViewer.Children.Count == 0 && StringHelper.isBlank(configSearch))
                {
                    drawingNginxConfig();
                }
                this.configHostViewer.ScrollToTop();
                this.configHostViewer.Content = configViewer;
            }
            if (isHostConfig)
            {
                if (!StringHelper.isBlank(this.searchText.Text))
                {
                    configSearch = this.searchText.Text;
                }
                if (hostSearch != null)
                {
                    this.searchText.Text = hostSearch;
                    drawingHostConfig(hostSearch);
                    hostSearch = null;
                    this.searchText.Visibility = Visibility.Visible;
                }
                else
                {
                    this.searchText.Text = "";
                    this.searchText.Visibility = Visibility.Hidden;
                }
            }
            else
            {
                if (!StringHelper.isBlank(this.searchText.Text))
                {
                    hostSearch = this.searchText.Text;
                }
                if (configSearch != null)
                {
                    this.searchText.Text = configSearch;
                    drawingNginxConfig(configSearch);
                    configSearch = null;
                    this.searchText.Visibility = Visibility.Visible;
                }
                else
                {
                    this.searchText.Text = "";
                    this.searchText.Visibility = Visibility.Hidden;
                }
            }
        }

        private void CommonMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (this.searchText.IsVisible && StringHelper.isBlank(this.searchText.Text))
            {
                this.searchText.Text = "";
                this.searchText.Visibility = Visibility.Hidden;
            }
        }
        #endregion

        #region config ui

        private void drawingNginxConfig()
        {
            this.searchText.Text = "";
            drawingNginxConfig(null);
        }

        private void drawingNginxConfig(string search)
        {
            // 渲染 nginx config
            string prefix = "-prod-";
            configViewer.Children.Clear();
            OdyConfigHelper.sortUpstream(currentEnv.Upstreams);
            foreach (var u in currentEnv.Upstreams)
            {
                UpstreamDetails ud;
                upstreamDetailsMap.TryGetValue(u.ServerName, out ud);
                if (search != null && filterSearch(u, ud, search))
                {
                    continue;
                }
                HashSet<string> contextPaths = new HashSet<string>();
                if (ud != null)
                {
                    ud.ContextPaths.ForEach(name => contextPaths.Add(name));
                }
                if (contextPaths.Count == 0)
                {
                    int index;
                    if ((index = u.ServerName.IndexOf(prefix)) > 0)
                    {
                        contextPaths.Add(u.ServerName.Substring(index + prefix.Length));
                    }
                    else
                    {
                        contextPaths.Add(u.ServerName);
                    }
                }
                foreach (var contextPath in contextPaths)
                {
                    configViewer.Children.Add(drawingConfig(u, ud, contextPath));
                }
            }
            if (!isHostConfig)
            {
                this.configHostViewer.Content = configViewer;
            }
        }

        private Border drawingConfig(NginxUpstream u, UpstreamDetails ud, string contextPath)
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

            dockRoot.MouseLeftButtonUp += CommonMouseLeftButtonUp;

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
            ipText.TextChanged += ConfigIpText_TextChanged;
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
                Width = 120
            };
            DockPanel.SetDock(dockPort, Dock.Left);
            Button localBut = new Button
            {
                Width = 42,
                Height = 28,
                Content = "local",
                Cursor = Cursors.Hand,
                ToolTip = "Set to local IP",
                HorizontalAlignment = HorizontalAlignment.Center,
                Background = new SolidColorBrush(global::OdyHostNginx.Resources.butInitColor),
                Foreground = new SolidColorBrush(global::OdyHostNginx.Resources.configFontColor)
            };
            localBut.DataContext = u;
            localBut.Click += LocalBut_Click;
            dockLocal.MouseLeftButtonDown += CommonMouseLeftButtonUp;
            dockLocal.Children.Add(localBut);
            // empty
            DockPanel emptyDock = new DockPanel();
            DockPanel.SetDock(emptyDock, Dock.Left);

            dockRoot.Children.Add(dockServer);
            dockRoot.Children.Add(dockIp);
            dockRoot.Children.Add(dockPort);
            dockRoot.Children.Add(dockLocal);
            dockRoot.Children.Add(emptyDock);

            border.Child = dockRoot;
            return border;
        }
        #endregion

        #region config activity

        private bool filterSearch(NginxUpstream u, UpstreamDetails ud, string search)
        {
            if (StringHelper.isBlank(search))
            {
                return false;
            }
            search = search.Trim();
            if (ud != null && search.StartsWith("/"))
            {
                foreach (var uri in ud.Uris)
                {
                    if (uri.Contains(search))
                    {
                        return false;
                    }
                }
            }
            else if (u.ServerName.Contains(search) || u.Ip.Contains(search))
            {
                return false;
            }
            else if (ud != null)
            {
                foreach (var item in ud.ContextPaths)
                {
                    if (item.Contains(search))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private void changeEnv(EnvConfig env, bool isCurrent)
        {
            if (env == null)
            {
                return;
            }
            string key = envKey(env);
            CheckBox switchBox;
            envSwitchUI.TryGetValue(key, out switchBox);
            if (switchBox != null)
            {
                DockPanel dockRoot = (DockPanel)VisualTreeHelper.GetParent(VisualTreeHelper.GetParent(switchBox));
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
            CommonMouseLeftButtonUp(null, null);
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

        private void ConfigIpText_TextChanged(object sender, TextChangedEventArgs e)
        {
            CommonMouseLeftButtonUp(null, null);
            TextBox ip = sender as TextBox;
            bool isIp = StringHelper.isIp(ip.Text.Trim());
            if (isIp)
            {
                NginxUpstream u = ip.DataContext as NginxUpstream;
                u.Ip = ip.Text.Trim();
                this.applyBut.Source = global::OdyHostNginx.Resources.img_can_apply;
            }
            ip.Foreground = new SolidColorBrush(isIp ? global::OdyHostNginx.Resources.configFontColor : Colors.Red);
        }
        #endregion

        #region host ui

        private void drawingHostConfig()
        {
            drawingHostConfig(null);
        }

        private void drawingHostConfig(string search)
        {
            // 渲染 host config
            hostViewer.Children.Clear();
            GroupBox userHost = null, envHost = null;

            if (userHosts != null && userHosts.Count > 0)
            {
                userHost = hostGroupBox(userHosts, "User Host", true, search);
            }
            if (currentEnv != null && currentEnv.Hosts != null && currentEnv.Hosts.Count > 0)
            {
                envHost = hostGroupBox(currentEnv.Hosts, "Env Host", false, search);
            }
            if (userHost != null)
            {
                hostViewer.Children.Add(userHost);
            }
            if (envHost != null)
            {
                hostViewer.Children.Add(envHost);
            }
            if (isHostConfig)
            {
                this.configHostViewer.Content = hostViewer;
            }
        }

        private GroupBox hostGroupBox(List<HostConfig> hosts, string header, bool isUserHost, string search)
        {
            OdyConfigHelper.sortHosts(hosts);
            GroupBox group = new GroupBox
            {
                Header = header,
                Margin = new Thickness(5, 10, 5, 0),
                Foreground = new SolidColorBrush(global::OdyHostNginx.Resources.hostFontColor)
            };
            StackPanel panel = new StackPanel
            {
                Orientation = Orientation.Vertical
            };
            foreach (var host in hosts)
            {
                if (!StringHelper.isBlank(search) && !(host.Domain + host.Ip).Contains(search))
                {
                    continue;
                }
                Border border = new Border
                {
                    Margin = new Thickness(5, 5, 5, 0),
                    BorderThickness = new Thickness(1, 1, 1, 1),
                    BorderBrush = new SolidColorBrush(global::OdyHostNginx.Resources.hostBorderColor)
                };
                DockPanel rootDock = new DockPanel
                {
                    Height = 30
                };
                rootDock.MouseLeftButtonUp += CommonMouseLeftButtonUp;
                // domain
                DockPanel domainDock = new DockPanel
                {
                    Width = 220
                };
                DockPanel.SetDock(domainDock, Dock.Left);
                TextBox domainText = new TextBox
                {
                    FontSize = 12,
                    Text = host.Domain,
                    ToolTip = host.Domain,
                    BorderThickness = new Thickness(0, 0, 0, 0),
                    VerticalContentAlignment = VerticalAlignment.Center,
                    HorizontalContentAlignment = HorizontalAlignment.Right,
                    Background = new SolidColorBrush(global::OdyHostNginx.Resources.hostBgColor),
                    Foreground = new SolidColorBrush(global::OdyHostNginx.Resources.hostFontColor)
                };
                domainText.DataContext = host;
                domainText.TextChanged += DomainText_TextChanged;
                domainDock.Children.Add(domainText);
                // 分割线
                DockPanel labelDock = new DockPanel
                {
                    Width = 80
                };
                DockPanel.SetDock(labelDock, Dock.Left);
                Label label = new Label
                {
                    FontSize = 14,
                    Content = "==>",
                    FontWeight = FontWeights.Bold,
                    BorderThickness = new Thickness(0, 0, 0, 0),
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Foreground = new SolidColorBrush(global::OdyHostNginx.Resources.hostFontColor)
                };
                labelDock.Children.Add(label);
                // ip
                DockPanel ipDock = new DockPanel
                {
                    Width = 120
                };
                DockPanel.SetDock(ipDock, Dock.Left);
                TextBox ipText = new TextBox
                {
                    FontSize = 14,
                    Text = host.Ip,
                    ToolTip = host.Ip,
                    BorderThickness = new Thickness(0, 0, 0, 0),
                    VerticalContentAlignment = VerticalAlignment.Center,
                    HorizontalContentAlignment = HorizontalAlignment.Left,
                    Background = new SolidColorBrush(global::OdyHostNginx.Resources.hostBgColor),
                    Foreground = new SolidColorBrush(global::OdyHostNginx.Resources.hostFontColor)
                };
                ipText.DataContext = host;
                ipText.TextChanged += HostIpText_TextChanged;
                ipDock.Children.Add(ipText);
                // switch / del
                DockPanel butDock = new DockPanel
                {
                    Width = 100
                };
                DockPanel.SetDock(ipDock, Dock.Left);
                if (isUserHost)
                {
                    Image delImage = new Image
                    {
                        Width = 22,
                        Cursor = Cursors.Hand,
                        ToolTip = "delete user host",
                        Margin = new Thickness(5, 0, 5, 0),
                        Source = global::OdyHostNginx.Resources.img_del_grey
                    };
                    delImage.DataContext = host;
                    delImage.MouseEnter += DelImage_MouseEnter;
                    delImage.MouseLeave += DelImage_MouseLeave;
                    delImage.MouseLeftButtonUp += DelImage_MouseLeftButtonUp;
                    butDock.Children.Add(delImage);
                } else
                {
                    Image addImage = new Image
                    {
                        Width = 22,
                        Cursor = Cursors.Hand,
                        ToolTip = "add to user host",
                        Margin = new Thickness(5, 0, 5, 0),
                        Source = global::OdyHostNginx.Resources.img_add_grey
                    };
                    addImage.DataContext = host;
                    addImage.MouseEnter += AddImage_MouseEnter;
                    addImage.MouseLeave += AddImage_MouseLeave;
                    addImage.MouseLeftButtonUp += AddImage_MouseLeftButtonUp;
                    butDock.Children.Add(addImage);
                }
                CheckBox hostSwitch = new CheckBox
                {
                    IsChecked = host.Use,
                    Cursor = Cursors.Hand,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Style = global::OdyHostNginx.Resources.switchButStyle
                };
                hostSwitch.DataContext = host;
                hostSwitch.Click += HostSwitch_Click;
                butDock.Children.Add(hostSwitch);
                // empty
                DockPanel emptyDock = new DockPanel();
                DockPanel.SetDock(emptyDock, Dock.Left);

                rootDock.Children.Add(domainDock);
                rootDock.Children.Add(labelDock);
                rootDock.Children.Add(ipDock);
                rootDock.Children.Add(butDock);
                rootDock.Children.Add(emptyDock);

                border.Child = rootDock;
                panel.Children.Add(border);
            }
            group.Content = panel;
            return group;
        }
        #endregion

        #region host activity

        private void HostSwitch_Click(object sender, RoutedEventArgs e)
        {
            CommonMouseLeftButtonUp(null, null);
            CheckBox envSwitch = sender as CheckBox;
            HostConfig host = envSwitch.DataContext as HostConfig;
            host.Use = envSwitch.IsChecked == true ? true : false;
            this.applyBut.Source = global::OdyHostNginx.Resources.img_can_apply;
        }

        private void HostIpText_TextChanged(object sender, TextChangedEventArgs e)
        {
            CommonMouseLeftButtonUp(null, null);
            TextBox ip = sender as TextBox;
            bool isIp = StringHelper.isIp(ip.Text.Trim());
            if (isIp)
            {
                HostConfig host = ip.DataContext as HostConfig;
                host.Ip = ip.Text.Trim();
                this.applyBut.Source = global::OdyHostNginx.Resources.img_can_apply;
            }
            ip.Foreground = new SolidColorBrush(isIp ? global::OdyHostNginx.Resources.hostFontColor : Colors.Red);
        }

        private void DomainText_TextChanged(object sender, TextChangedEventArgs e)
        {
            CommonMouseLeftButtonUp(null, null);
            TextBox domain = sender as TextBox;
            bool isDomain = StringHelper.isDomain(domain.Text.Trim());
            if (isDomain)
            {
                HostConfig host = domain.DataContext as HostConfig;
                host.Domain = domain.Text.Trim();
                this.applyBut.Source = global::OdyHostNginx.Resources.img_can_apply;
            }
            domain.Foreground = new SolidColorBrush(isDomain ? global::OdyHostNginx.Resources.hostFontColor : Colors.Red);
        }

        private void DelImage_MouseEnter(object sender, MouseEventArgs e)
        {
            Image delImage = sender as Image;
            delImage.Source = global::OdyHostNginx.Resources.img_del_red;
        }

        private void DelImage_MouseLeave(object sender, MouseEventArgs e)
        {
            Image delImage = sender as Image;
            delImage.Source = global::OdyHostNginx.Resources.img_del_grey;
        }

        private void DelImage_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            CommonMouseLeftButtonUp(null, null);
            Image delImage = sender as Image;
            HostConfig host = delImage.DataContext as HostConfig;
            foreach (var item in userHosts)
            {
                if (host == item)
                {
                    userHosts.Remove(host);
                    this.applyBut.Source = global::OdyHostNginx.Resources.img_can_apply;
                    drawingHostConfig();
                    break;
                }
            }
        }

        private void AddImage_MouseEnter(object sender, MouseEventArgs e)
        {
            Image addImage = sender as Image;
            addImage.Source = global::OdyHostNginx.Resources.img_add_blue;
        }

        private void AddImage_MouseLeave(object sender, MouseEventArgs e)
        {
            Image addImage = sender as Image;
            addImage.Source = global::OdyHostNginx.Resources.img_add_grey;
        }

        private void AddImage_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            CommonMouseLeftButtonUp(null, null);
            Image addImage = sender as Image;
            HostConfig host = addImage.DataContext as HostConfig;
            userHosts.Add(new HostConfig()
            {
                Use = false,
                Ip = host.Ip,
                PingIp = host.PingIp,
                Domain = host.Domain
            });
            this.applyBut.Source = global::OdyHostNginx.Resources.img_can_apply;
            drawingHostConfig();
        }
        #endregion

        #region 底部按钮
        private void AddBut_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (isHostConfig)
            {
                ConfigDialogData.success = false;
                HostConfigWindows hostConfigWindows = new HostConfigWindows();
                hostConfigWindows.ShowDialog();
                if (ConfigDialogData.success)
                {
                    HostConfig host = new HostConfig
                    {
                        Ip = ConfigDialogData.ip,
                        Domain = ConfigDialogData.domain
                    };
                    userHosts.Add(host);
                    drawingHostConfig();
                    this.applyBut.Source = global::OdyHostNginx.Resources.img_can_apply;
                }
            }
        }

        private void SearchBut_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.searchText.Visibility = Visibility.Visible;
            this.searchText.Select(0, 1);
        }

        private void SearchText_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!this.searchText.IsVisible)
            {
                return;
            }
            if (isHostConfig)
            {
                // 搜索 host
                drawingHostConfig(this.searchText.Text);
            }
            else
            {
                // 搜索 config
                drawingNginxConfig(this.searchText.Text);
            }
        }

        private void ResetBut_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (isHostConfig)
            {
                userHosts = OdyConfigHelper.loadUserHosts();
            }
            else if (currentEnv != null)
            {
                OdyConfigHelper.reloadEnv(currentEnv, false);
            }
            drawingEnvConfig(currentEnv, true);
            apply();
        }

        private void ApplyBut_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            apply();
        }
        #endregion

    }
}
