using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OdyHostNginx
{
    class TableVo
    {

        private string className;
        private string tableName;
        private string tableComment;
        private List<FieldVo> fields;
        private bool check;

        public string ClassName { get => className; set => className = value; }
        public string TableName { get => tableName; set => tableName = value; }
        public string TableComment { get => tableComment; set => tableComment = value; }
        internal List<FieldVo> Fields { get => fields; set => fields = value; }
        public bool Check { get => check; set => check = value; }

        public FieldVo getMainField()
        {
            foreach (var field in fields)
            {
                if (field.isPRI())
                {
                    return field;
                }
            }
            return new FieldVo()
            {
                Extra = "auto_increment",
                ColumnName = "id",
                ColumnKey = "PRI",
                DataType = "bigint",
                ColumnType = "bigint(20)"
            };
        }

    }
}
