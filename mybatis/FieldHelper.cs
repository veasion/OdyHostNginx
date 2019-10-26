using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OdyHostNginx
{
    class FieldHelper
    {

        private static Dictionary<string, string> javaTypeDic = null;
        private static Dictionary<string, string> javaPackageDic = null;

        static FieldHelper()
        {
            javaTypeDic = new Dictionary<string, string>();
            javaPackageDic = new Dictionary<string, string>();
            javaTypeDic["varchar"] = "String";
            javaPackageDic["varchar"] = "java.lang.String";
            javaTypeDic["char"] = "String";
            javaPackageDic["char"] = "java.lang.String";
            javaTypeDic["blob"] = "Byte[]";
            javaPackageDic["blob"] = "java.lang.Byte[]";
            javaTypeDic["text"] = "String";
            javaPackageDic["text"] = "java.lang.String";
            javaTypeDic["bigint"] = "Long";
            javaPackageDic["bigint"] = "java.lang.Long";
            javaTypeDic["integer"] = "Long";
            javaPackageDic["integer"] = "java.lang.Long";
            javaTypeDic["id"] = "Long";
            javaPackageDic["id"] = "java.lang.Long";
            javaTypeDic["int"] = "Integer";
            javaPackageDic["int"] = "java.lang.Integer";
            javaTypeDic["tinyint"] = "Integer";
            javaPackageDic["tinyint"] = "java.lang.Integer";
            javaTypeDic["boolean"] = "Integer";
            javaPackageDic["boolean"] = "java.lang.Integer";
            javaTypeDic["smallint"] = "Integer";
            javaPackageDic["smallint"] = "java.lang.Integer";
            javaTypeDic["mediumint"] = "Integer";
            javaPackageDic["mediumint"] = "java.lang.Integer";
            javaTypeDic["bit"] = "Boolean";
            javaPackageDic["bit"] = "java.lang.Boolean";
            javaTypeDic["float"] = "Float";
            javaPackageDic["float"] = "java.lang.Float";
            javaTypeDic["double"] = "Double";
            javaPackageDic["double"] = "java.lang.Double";
            javaTypeDic["decimal"] = "BigDecimal";
            javaPackageDic["decimal"] = "java.math.BigDecimal";
            javaTypeDic["date"] = "Date";
            javaPackageDic["date"] = "java.util.Date";
            javaTypeDic["time"] = "Date";
            javaPackageDic["time"] = "java.util.Date";
            javaTypeDic["datetime"] = "Date";
            javaPackageDic["datetime"] = "java.util.Date";
            javaTypeDic["timestamp"] = "Date";
            javaPackageDic["timestamp"] = "java.util.Date";
            javaTypeDic["year"] = "Date";
            javaPackageDic["year"] = "java.util.Date";
        }

        public static string javaType(string fieldType)
        {
            fieldType = fieldType.Trim().ToLower();
            if (javaTypeDic.ContainsKey(fieldType))
            {
                return javaTypeDic[fieldType];
            }
            return "String";
        }

        public static string javaPackage(string fieldType)
        {
            fieldType = fieldType.Trim().ToLower();
            if (javaPackageDic.ContainsKey(fieldType))
            {
                return javaPackageDic[fieldType];
            }
            return "java.lang.String";
        }

    }
}
