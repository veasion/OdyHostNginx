using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OdyHostNginx
{
    class FieldVo
    {

        private string extra;
        private string dataType;
        private string fieldName;
        private string columnKey;
        private string columnType;
        private string columnName;
        private string columnComment;

        public string getJavaType()
        {
            return FieldHelper.javaType(DataType);
        }

        public string getImportPackages()
        {
            return FieldHelper.javaPackage(DataType);
        }

        public bool isPRI()
        {
            return columnKey != null ? "PRI".Equals(columnKey.Trim().ToUpper()) : false;
        }

        public bool isAutoIncrement()
        {
            return extra != null ? "auto_increment".Equals(extra.Trim().ToLower()) : false;
        }

        public bool isMUL()
        {
            return columnKey != null ? "MUL".Equals(columnKey.Trim().ToUpper()) : false;
        }

        public string UpFieldName()
        {
            return StringHelper.upFirst(fieldName);
        }

        public string Extra { get => extra; set => extra = value; }
        public string DataType { get => dataType; set => dataType = value; }
        public string FieldName { get => fieldName; set => fieldName = value; }
        public string ColumnKey { get => columnKey; set => columnKey = value; }
        public string ColumnType { get => columnType; set => columnType = value; }
        public string ColumnName { get => columnName; set => columnName = value; }
        public string ColumnComment { get => columnComment; set => columnComment = value; }

    }
}
