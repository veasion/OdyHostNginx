using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace OdyHostNginx
{
    class DBUtils
    {

        static HashSet<string> sysDatabases;
        static string dbConfigPath = FileHelper.getCurrentDirectory() + "\\bin\\config\\db.json";

        static DBUtils()
        {
            sysDatabases = new HashSet<string>();
            sysDatabases.Add("sys");
            sysDatabases.Add("mysql");
            sysDatabases.Add("information_schema");
            sysDatabases.Add("performance_schema");
        }

        public static DbConnection openConnection(string ip, int port, string user, string pwd, string db)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("server=").Append(ip).Append(";");
            sb.Append("port=").Append(port).Append(";");
            sb.Append("user=").Append(user).Append(";");
            sb.Append("password=").Append(pwd).Append(";");
            if (!StringHelper.isEmpty(db))
            {
                sb.Append("database=").Append(db).Append(";");
            }
            DbConnection conn = new MySqlConnection(sb.ToString());
            try
            {
                conn.Open();
                saveConfig(ip, port, user, pwd);
            }
            catch (Exception e)
            {
                Logger.error("创建数据库连接", e);
                MessageBox.Show("连接数据库失败！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return conn;
        }

        public static void closeConnection(DbConnection conn)
        {
            try
            {
                if (conn != null)
                {
                    conn.Close();
                }
            }
            catch (Exception) { }
        }

        public static List<DbCombox> queryDatabases(DbConnection conn)
        {
            string sql = "show databases";
            List<DbCombox> list = new List<DbCombox>();
            try
            {
                MySqlCommand cmd = new MySqlCommand(sql, (MySqlConnection)conn);

                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    string db = reader.GetString("Database");
                    if (sysDatabases == null || !sysDatabases.Contains(db))
                    {
                        list.Add(new DbCombox(db, db));
                    }
                }
                reader.Close();
            }
            catch (Exception e)
            {
                Logger.error("加载数据库", e);
            }
            return list;
        }

        public static List<TableVo> loadTables(DbConnection conn, string db, bool queryFields)
        {
            List<TableVo> tables = queryTables(conn, db, null);
            if (queryFields)
            {
                loadFields(conn, db, tables);
            }
            return tables;
        }

        public static void loadFields(DbConnection conn, string db, List<TableVo> tables)
        {
            if (tables != null)
            {
                foreach (var table in tables)
                {
                    table.Fields = queryFields(conn, db, table.TableName);
                }
            }
        }

        public static List<TableVo> queryTables(DbConnection conn, string db, string filter)
        {
            string sql = "select table_name, table_comment from information_schema.tables where table_schema= @db";
            if (!StringHelper.isEmpty(filter))
            {
                sql += " and table_name like concat('%',@filter,'%')";
            }
            MySqlCommand cmd = new MySqlCommand(sql, (MySqlConnection)conn);
            cmd.Parameters.AddWithValue("db", db);
            if (!StringHelper.isEmpty(filter))
            {
                cmd.Parameters.AddWithValue("filter", filter);
            }
            List<TableVo> list = new List<TableVo>();
            MySqlDataReader reader = cmd.ExecuteReader();
            TableVo table;
            while (reader.Read())
            {
                table = new TableVo();
                table.TableName = reader.GetString("table_name");
                table.TableComment = reader.GetString("table_comment");
                table.ClassName = StringHelper.upFirst(StringHelper.toVar(table.TableName));
                list.Add(table);
            }
            reader.Close();
            return list;
        }

        public static List<FieldVo> queryFields(DbConnection conn, string db, string table)
        {
            string sql = "select column_name, column_comment, data_type, column_type, column_key, extra " +
                "from information_schema.columns where table_schema = @db and table_name = @table";
            MySqlCommand cmd = new MySqlCommand(sql, (MySqlConnection)conn);
            cmd.Parameters.AddWithValue("db", db);
            cmd.Parameters.AddWithValue("table", table);
            List<FieldVo> list = new List<FieldVo>();
            MySqlDataReader reader = cmd.ExecuteReader();
            FieldVo field;
            while (reader.Read())
            {
                field = new FieldVo();
                field.Extra = reader.GetString("extra");
                field.DataType = reader.GetString("data_type");
                field.ColumnKey = reader.GetString("column_key");
                field.ColumnType = reader.GetString("column_type");
                field.ColumnName = reader.GetString("column_name");
                field.ColumnComment = reader.GetString("column_comment");
                field.FieldName = StringHelper.toVar(field.ColumnName);
                list.Add(field);
            }
            reader.Close();
            return list;
        }

        public static void saveConfig(string ip, int port, string user, string pwd)
        {
            ConnectionParams connParams = new ConnectionParams();
            connParams.Ip = ip;
            connParams.Port = port;
            connParams.User = user;
            connParams.Pwd = pwd;
            try
            {
                FileHelper.writeJsonFile(connParams, dbConfigPath);
            }
            catch (Exception) { }
        }

        public static ConnectionParams readConfig()
        {
            try
            {
                return FileHelper.readJsonFile<ConnectionParams>(dbConfigPath);
            }
            catch (Exception) { }
            return null;
        }

    }
}
