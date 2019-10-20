using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace OdyHostNginx
{
    /// <summary>
    /// Mybatis Generate
    /// </summary>
    public partial class MybatisGenerateWindows : Window
    {

        private string db;
        private DbConnection conn;
        private Generate generate;
        private List<TableVo> tables;
        private List<TextBox> classTexts = new List<TextBox>();
        private List<Border> tableBorders = new List<Border>();

        public MybatisGenerateWindows()
        {
            InitializeComponent();
            ConnectionParams connParams = DBUtils.readConfig();
            if (connParams != null)
            {
                pwdText.Text = connParams.Pwd;
                userText.Text = connParams.User;
                serverText.Text = connParams.Ip;
                portText.Text = connParams.Port + "";
            }
        }

        private void Load_Click(object sender, RoutedEventArgs e)
        {
            Button loadBut = sender as Button;
            if (!StringHelper.isNumberic(portText.Text.Trim()))
            {
                portText.Text = "3306";
            }
            string ip = serverText.Text.Trim();
            int port = int.Parse(portText.Text.Trim());
            string user = userText.Text.Trim();
            string pwd = pwdText.Text.Trim();
            if (StringHelper.isEmpty(ip))
            {
                MessageBox.Show("数据库连接地址不能为空！", "提示");
                return;
            }
            if (StringHelper.isEmpty(user) || StringHelper.isEmpty(pwd))
            {
                MessageBox.Show("数据库用户名或密码不能为空！", "提示");
                return;
            }
            loadBut.IsEnabled = false;
            loadBut.Content = "Loading...";
            ThreadPool.QueueUserWorkItem(o =>
            {
                List<DbCombox> dbs = new List<DbCombox>();
                try
                {
                    if (conn != null)
                    {
                        DBUtils.closeConnection(conn);
                    }
                    conn = DBUtils.openConnection(ip, port, user, pwd, null);
                    dbs = DBUtils.queryDatabases(conn);
                }
                catch (Exception)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        loadBut.IsEnabled = true;
                        loadBut.Content = "Load";
                    });
                }
                Application.Current.Dispatcher.Invoke(() =>
                {
                    try
                    {
                        this.dbComboBox.ItemsSource = dbs;
                        this.dbComboBox.DisplayMemberPath = "Label";
                        this.dbComboBox.SelectedValuePath = "Value";
                        if (dbs != null && dbs.Count > 0)
                        {
                            this.dbComboBox.SelectedIndex = 0;
                            for (int i = 0; i < dbs.Count; i++)
                            {
                                if ("oms".Equals(dbs[i].Value))
                                {
                                    this.dbComboBox.SelectedIndex = i;
                                    break;
                                }
                            }
                            changeDb(this.dbComboBox.SelectedItem as DbCombox);
                        }
                    }
                    finally
                    {
                        loadBut.IsEnabled = true;
                        loadBut.Content = "Load";
                    }
                });
            });
        }

        private void DbComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            changeDb(this.dbComboBox.SelectedItem as DbCombox);
        }

        private void changeDb(DbCombox dbCombox)
        {
            if (dbCombox == null) return;
            this.filterText.Text = "";
            this.filterText.IsEnabled = true;
            this.db = dbCombox.Value;
            tables = DBUtils.loadTables(conn, db, false);
            generate = Generate.GetGenerate(db);
            this.tablePrefixText.Text = generate.TablePrefix;
            this.poSuffixText.Text = generate.PoSuffix;
            this.voSuffixText.Text = generate.VoSuffix;
            this.mapperSuffixText.Text = generate.MapperSuffix;
            this.poPackageText.Text = generate.PoPackage;
            this.voPackageText.Text = generate.VoPackage;
            this.mapperPackageText.Text = generate.MapperPackage;
            if (tables != null)
            {
                foreach (var table in tables)
                {
                    table.ClassName = className(table.TableName, generate.TablePrefix);
                }
            }
            drawingTables();
            drawingTablesUI(this.filterText.Text);
        }

        private void drawingTables()
        {
            classTexts.Clear();
            tableBorders.Clear();
            if (tables == null || tables.Count == 0) return;
            foreach (var table in tables)
            {
                Border border = new Border
                {
                    DataContext = table,
                    Margin = new Thickness(5, 10, 5, 0),
                    BorderThickness = new Thickness(1, 1, 1, 1),
                    BorderBrush = new SolidColorBrush(OdyResources.configBorderColor)
                };
                DockPanel dockRoot = new DockPanel
                {
                    Height = 38
                };
                // table
                DockPanel dockTable = new DockPanel
                {
                    Width = 200
                };
                DockPanel.SetDock(dockTable, Dock.Left);
                Label tableLabel = new Label
                {
                    FontSize = 16,
                    ToolTip = table.TableName,
                    Content = table.TableName.Replace("_", "__"),
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Foreground = new SolidColorBrush(OdyResources.configFontColor)
                };
                dockTable.Children.Add(tableLabel);
                // check
                DockPanel dockCheck = new DockPanel
                {
                    Width = 80
                };
                CheckBox check = new CheckBox
                {
                    DataContext = table,
                    IsChecked = table.Check,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                check.Click += Check_Click;
                dockCheck.Children.Add(check);
                DockPanel.SetDock(dockCheck, Dock.Right);
                // class
                DockPanel dockClass = new DockPanel
                {
                    Width = 200
                };
                TextBox classText = new TextBox
                {
                    FontSize = 16,
                    DataContext = table,
                    Text = table.ClassName,
                    BorderThickness = new Thickness(1, 0, 1, 0),
                    Background = new SolidColorBrush(Colors.White),
                    VerticalContentAlignment = VerticalAlignment.Center,
                    HorizontalContentAlignment = HorizontalAlignment.Center,
                    Foreground = new SolidColorBrush(OdyResources.configFontColor)
                };
                classText.TextChanged += ClassText_TextChanged;
                classTexts.Add(classText);
                DockPanel.SetDock(dockClass, Dock.Right);
                dockClass.Children.Add(classText);
                // comment
                DockPanel dockComment = new DockPanel
                {
                    Width = 180
                };
                Label commentLabel = new Label
                {
                    FontSize = 16,
                    Content = table.TableComment,
                    ToolTip = table.TableComment,
                    BorderThickness = new Thickness(1, 0, 0, 0),
                    VerticalContentAlignment = VerticalAlignment.Center,
                    HorizontalContentAlignment = HorizontalAlignment.Left,
                    Foreground = new SolidColorBrush(OdyResources.configFontColor),
                    BorderBrush = new SolidColorBrush(OdyResources.configBorderColor)
                };
                DockPanel.SetDock(dockComment, Dock.Left);
                dockComment.Children.Add(commentLabel);

                dockRoot.Children.Add(dockTable);
                dockRoot.Children.Add(dockCheck);
                dockRoot.Children.Add(dockClass);
                dockRoot.Children.Add(dockComment);

                border.Child = dockRoot;
                tableBorders.Add(border);
            }
        }

        private void drawingTablesUI(string filter)
        {
            tablesPanel.Children.Clear();
            if (tableBorders == null || tableBorders.Count == 0) return;
            tableBorders.Sort((o1, o2) =>
            {
                if (o1 == null || o2 == null) return 1;
                TableVo t1 = o1.DataContext as TableVo;
                TableVo t2 = o2.DataContext as TableVo;
                if (t1.Check && !t2.Check)
                {
                    return -1;
                }
                else if (t2.Check && !t1.Check)
                {
                    return 1;
                }
                else
                {
                    return t1.TableName.Length > t2.TableName.Length ? 1 : -1;
                }
            });
            TableVo table = null;
            foreach (var item in tableBorders)
            {
                table = item.DataContext as TableVo;
                if (StringHelper.isEmpty(filter)
                    || table.TableName.Contains(filter.Trim())
                    || (StringHelper.hasChinese(filter) && table.TableComment.Contains(filter.Trim())))
                {
                    tablesPanel.Children.Add(item);
                }
            }
        }

        private void ClassText_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            (textBox.DataContext as TableVo).ClassName = textBox.Text;
        }

        private void Check_Click(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            (checkBox.DataContext as TableVo).Check = checkBox.IsChecked == true;
        }

        private void TablePrefixText_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tablePrefixText = sender as TextBox;
            string prefix = tablePrefixText.Text.Trim();
            if (classTexts == null) return;
            foreach (var classText in classTexts)
            {
                TableVo table = classText.DataContext as TableVo;
                table.ClassName = className(table.TableName, prefix);
                classText.Text = table.ClassName;
            }
        }

        private string className(string tableName, string prefix)
        {
            if (!StringHelper.isEmpty(tableName) && tableName.StartsWith(prefix))
            {
                tableName = tableName.Substring(prefix.Length);
                if ("".Equals(tableName))
                {
                    tableName = prefix;
                }
            }
            return StringHelper.upFirst(StringHelper.toVar(tableName));
        }

        private void Generate_Click(object sender, RoutedEventArgs e)
        {
            if (generate == null)
            {
                MessageBox.Show("请加载并选择数据库！", "提示");
                return;
            }
            generate.TablePrefix = this.tablePrefixText.Text.Trim();
            generate.PoSuffix = this.poSuffixText.Text.Trim();
            generate.VoSuffix = this.voSuffixText.Text.Trim();
            generate.MapperSuffix = this.mapperSuffixText.Text.Trim();
            generate.PoPackage = this.poPackageText.Text.Trim();
            generate.VoPackage = this.voPackageText.Text.Trim();
            generate.MapperPackage = this.mapperPackageText.Text.Trim();
            List<TableVo> tableList = tables.Where(w => w.Check).ToList();
            if (tableList == null || tableList.Count == 0)
            {
                MessageBox.Show("请选择将要生成的表！", "提示");
                return;
            }
            string dir = FileHelper.getDesktopDirectory() + "\\MybtaisGenerate";
            try
            {
                DBUtils.loadFields(conn, db, tableList);
                MybtaisGenerate.generate(generate, tableList).saveTo(dir);
                MessageBox.Show("文件已生成到桌面！\r\n\r\n路径：" + dir, "提示");
            }
            catch (Exception ex)
            {
                MessageBox.Show("生成失败！错误：" + ex.Message, "错误");
            }
        }

        private void FilterText_TextChanged(object sender, TextChangedEventArgs e)
        {
            drawingTablesUI(this.filterText.Text);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (conn == null) return;
            try
            {
                DBUtils.closeConnection(conn);
            }
            catch (Exception) { }
        }

    }
}
