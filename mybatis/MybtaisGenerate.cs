using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OdyHostNginx
{
    class MybtaisGenerate
    {

        private const int fieldCount = 4;
        private const string space4 = "    ";
        private const string space8 = space4 + space4;
        private const string space12 = space8 + space4;
        private const string space16 = space12 + space4;

        public static Result generate(Generate generate, List<TableVo> tables)
        {
            List<FileVO> pos = new List<FileVO>();
            List<FileVO> vos = new List<FileVO>();
            List<FileVO> xmls = new List<FileVO>();
            List<FileVO> mappers = new List<FileVO>();
            foreach (var table in tables)
            {
                string poClassName = table.ClassName + generate.PoSuffix;
                string voClassName = table.ClassName + generate.VoSuffix;
                string mapperClassName = table.ClassName + generate.MapperSuffix;
                pos.Add(new FileVO()
                {
                    Body = bean(table, generate.PoPackage, poClassName, false),
                    FileName = poClassName + ".java"
                });
                vos.Add(new FileVO()
                {
                    Body = bean(table, generate.VoPackage, voClassName, true),
                    FileName = voClassName + ".java"
                });
                mappers.Add(mapper(table, generate.MapperPackage, mapperClassName, generate.PoPackage, poClassName));
                xmls.Add(xml(table, generate.MapperPackage, mapperClassName, generate.PoPackage, poClassName));
            }
            return new Result()
            {
                Pos = pos,
                Vos = vos,
                Xmls = xmls,
                Mappers = mappers
            };
        }

        private static string bean(TableVo table, string package, string className, bool swagger)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("package ").Append(package).AppendLine(";");
            sb.AppendLine();
            import(sb, "java.io.Serializable");
            if (swagger)
            {
                import(sb, "io.swagger.annotations.ApiModel");
                import(sb, "io.swagger.annotations.ApiModelProperty");
            }
            HashSet<string> packages = new HashSet<string>();
            table.Fields.ForEach(f => packages.Add(f.getImportPackages()));
            foreach (var p in packages)
            {
                import(sb, p);
            }
            sb.AppendLine();
            comment(sb, table.TableComment, false);
            if (swagger)
            {
                sb.AppendLine("@ApiModel");
            }
            sb.Append("public class ").Append(className);
            sb.AppendLine(" implements Serializable {");
            sb.AppendLine();
            sb.Append(space4).AppendLine("private static final long serialVersionUID = -1L;");
            sb.AppendLine();
            foreach (var f in table.Fields)
            {
                comment(sb, f.ColumnComment, true);
                if (swagger)
                {
                    sb.Append(space4);
                    sb.AppendFormat("@ApiModelProperty(\"{0}\")", fieldComment(f));
                    sb.AppendLine();
                }
                field(sb, f);
            }
            sb.AppendLine();
            foreach (var f in table.Fields)
            {
                getset(sb, f);
            }
            sb.AppendLine("}");
            return sb.ToString();
        }

        private static void import(StringBuilder sb, string classFullName)
        {
            if (StringHelper.isEmpty(classFullName)) return;
            if (classFullName.StartsWith("java.lang.")) return;
            sb.Append("import ").Append(classFullName).AppendLine(";");
        }

        private static void comment(StringBuilder sb, string comment, bool hasBlank)
        {
            if (StringHelper.isBlank(comment)) return;
            string blank = hasBlank ? space4 : "";
            sb.Append(blank).AppendLine("/**");
            sb.Append(blank).Append(" * ").AppendLine(comment);
            if (!hasBlank)
            {
                sb.Append(blank).AppendLine(" *");
                try
                {
                    sb.Append(blank).Append(" * @author ").AppendLine(System.Net.Dns.GetHostName());
                }
                catch (Exception) { }
                sb.Append(blank).Append(" * @date ").AppendLine(DateTime.Now.ToString("yyyy-MM-dd"));
            }
            sb.Append(blank).AppendLine(" */");
        }

        private static string fieldComment(FieldVo field)
        {
            return StringHelper.isBlank(field.ColumnComment) ? field.FieldName : field.ColumnComment.Replace("\"", "'");
        }

        private static void field(StringBuilder sb, FieldVo filed)
        {
            sb.Append(space4).Append("private ");
            sb.Append(filed.getJavaType()).Append(" ");
            sb.Append(filed.FieldName).AppendLine(";");
        }

        private static void getset(StringBuilder sb, FieldVo filed)
        {
            // get
            sb.Append(space4).Append("public ");
            sb.Append(filed.getJavaType()).Append(" get");
            sb.Append(filed.UpFieldName()).AppendLine("() {");
            sb.Append(space4).Append(space4);
            sb.Append("return ").Append(filed.FieldName);
            sb.AppendLine(";").Append(space4).AppendLine("}");
            sb.AppendLine();
            // set
            sb.Append(space4).Append("public void ");
            sb.Append("set").Append(filed.UpFieldName());
            sb.Append("(").Append(filed.getJavaType());
            sb.Append(" ").Append(filed.FieldName).AppendLine(") {");
            sb.Append(space4).Append(space4);
            sb.Append("this.").Append(filed.FieldName);
            sb.Append(" = ").Append(filed.FieldName).AppendLine(";");
            sb.Append(space4).AppendLine("}");
            sb.AppendLine();
        }

        private static FileVO mapper(TableVo table, string mapperPackage, string mapperClassName, string poPackage, string poClassName)
        {
            FieldVo pri = table.getMainField();
            StringBuilder sb = new StringBuilder();
            sb.Append("package ").Append(mapperPackage).AppendLine(";");
            sb.AppendLine();
            import(sb, "org.apache.ibatis.annotations.Mapper");
            import(sb, poPackage + "." + poClassName);
            import(sb, "java.util.List");
            import(sb, pri.getImportPackages());
            sb.AppendLine();
            comment(sb, table.TableComment, false);
            sb.AppendLine("@Mapper");
            sb.Append("public interface ");
            sb.Append(mapperClassName).AppendLine(" {");
            sb.AppendLine();
            // insert
            comment(sb, "新增", true);
            sb.Append(space4).Append("int insert(");
            sb.Append(poClassName).AppendLine(" obj);");
            sb.AppendLine();
            // insertAll
            comment(sb, "批量新增", true);
            sb.Append(space4).Append("int insertAll(");
            sb.Append("List<").Append(poClassName).AppendLine("> list);");
            sb.AppendLine();
            // update
            comment(sb, "修改不为空字段", true);
            sb.Append(space4).Append("int update(");
            sb.Append(poClassName).AppendLine(" obj);");
            sb.AppendLine();
            // updateAll
            comment(sb, "修改全部字段", true);
            sb.Append(space4).Append("int updateAll(");
            sb.Append(poClassName).AppendLine(" obj);");
            sb.AppendLine();
            // deleteById
            comment(sb, "删除", true);
            sb.Append(space4).Append("int deleteBy");
            sb.Append(pri.UpFieldName()).Append("(");
            sb.Append(pri.getJavaType()).Append(" ");
            sb.Append(pri.FieldName).AppendLine(");");
            sb.AppendLine();
            // queryById
            comment(sb, "根据" + pri.FieldName + "查询", true);
            sb.Append(space4).Append(poClassName).Append(" queryBy");
            sb.Append(pri.UpFieldName()).Append("(");
            sb.Append(pri.getJavaType()).Append(" ");
            sb.Append(pri.FieldName).AppendLine(");");
            sb.AppendLine();
            // queryList
            comment(sb, "查询list", true);
            sb.Append(space4).Append("List<").Append(poClassName);
            sb.AppendLine("> queryList();");
            sb.AppendLine();
            sb.AppendLine("}");
            return new FileVO()
            {
                Body = sb.ToString(),
                FileName = mapperClassName + ".java"
            };
        }

        private static FileVO xml(TableVo table, string mapperPackage, string mapperClassName, string poPackage, string poClassName)
        {
            FieldVo pri = table.getMainField();
            string poClassFullName = poPackage + "." + poClassName;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            sb.AppendLine("<!DOCTYPE mapper PUBLIC \" -//mybatis.org//DTD Mapper 3.0//EN\" \"http://mybatis.org/dtd/mybatis-3-mapper.dtd\">");
            sb.Append("<mapper namespace=\"").Append(mapperPackage).AppendLine("\">");
            sb.AppendLine();
            // insert
            sb.Append(space4).AppendLine("<insert id=\"insert\">");
            sb.Append(space8).Append("insert into ").AppendLine(table.TableName);
            sb.Append(space8).AppendLine("(");
            fields(sb, table, space12, false, null);
            sb.Append(space8).AppendLine(")");
            sb.Append(space8).AppendLine("values");
            sb.Append(space8).AppendLine("(");
            fields(sb, table, space12, true, null);
            sb.Append(space8).AppendLine(")");
            sb.Append(space4).AppendLine("</insert>");
            sb.AppendLine();
            // insertAll
            sb.Append(space4).AppendLine("<insert id=\"insertAll\">");
            sb.Append(space8).Append("insert into ").AppendLine(table.TableName);
            sb.Append(space8).AppendLine("(");
            fields(sb, table, space12, false, null);
            sb.Append(space8).AppendLine(")");
            sb.Append(space8).AppendLine("values");
            sb.Append(space8).AppendLine("<foreach collection=\"list\" item=\"obj\" separator=\",\">");
            sb.Append(space8).AppendLine("(");
            fields(sb, table, space12, true, "obj");
            sb.Append(space8).AppendLine(")");
            sb.Append(space8).AppendLine("</foreach>");
            sb.Append(space4).AppendLine("</insert>");
            sb.AppendLine();
            // update
            sb.Append(space4).AppendLine("<update id=\"update\">");
            sb.Append(space8).Append("update ").AppendLine(table.TableName);
            sb.Append(space8).AppendLine("<set>");
            foreach (var field in table.Fields)
            {
                if (field.isPRI()) continue;
                sb.Append(space12).Append("<if test=\"").Append(field.FieldName).AppendLine(" != null\">");
                sb.Append(space16).Append(field.ColumnName).Append(" = #{").Append(field.FieldName).AppendLine("},");
                sb.Append(space12).AppendLine("</if>");
            }
            sb.Append(space8).AppendLine("</set>");
            sb.Append(space8).Append("where ").Append(pri.ColumnName);
            sb.Append(" = #{").Append(pri.FieldName).AppendLine("}");
            sb.Append(space4).AppendLine("</update>");
            sb.AppendLine();
            // updateAll
            sb.Append(space4).AppendLine("<update id=\"updateAll\">");
            sb.Append(space8).Append("update ").AppendLine(table.TableName);
            sb.Append(space8).AppendLine("<set>");
            foreach (var field in table.Fields)
            {
                if (field.isPRI()) continue;
                sb.Append(space12).Append(field.ColumnName).Append(" = #{").Append(field.FieldName).AppendLine("},");
            }
            sb.Append(space8).AppendLine("</set>");
            sb.Append(space8).Append("where ").Append(pri.ColumnName);
            sb.Append(" = #{").Append(pri.FieldName).AppendLine("}");
            sb.Append(space4).AppendLine("</update>");
            sb.AppendLine();
            // deleteById
            sb.Append(space4).Append("<delete id=\"deleteBy");
            sb.Append(pri.UpFieldName()).AppendLine("\">");
            sb.Append(space8).Append("delete from ").Append(table.TableName);
            sb.Append(" where ").Append(pri.ColumnName).AppendLine(" = #{value}");
            sb.Append(space4).AppendLine("</delete>");
            sb.AppendLine();
            // 逻辑删除
            sb.Append(space4).AppendLine("<!-- 逻辑删除 -->");
            sb.Append(space4).AppendLine("<!--");
            sb.Append(space4).Append("<update id=\"deleteBy");
            sb.Append(pri.UpFieldName()).AppendLine("\">");
            sb.Append(space8).Append("update ").Append(table.TableName);
            sb.Append(" set is_deleted = 1 where ");
            sb.Append(pri.ColumnName).AppendLine(" = #{value}");
            sb.Append(space4).AppendLine("</update>");
            sb.Append(space4).AppendLine("-->");
            sb.AppendLine();
            // queryById
            sb.Append(space4).Append("<select id=\"queryBy");
            sb.Append(pri.UpFieldName()).Append("\" resultType=\"");
            sb.Append(poClassFullName).AppendLine("\">");
            sb.Append(space8).Append("select * from ").Append(table.TableName);
            sb.Append(" where ").Append(pri.ColumnName).AppendLine(" = #{value}");
            sb.Append(space4).AppendLine("</select>");
            sb.AppendLine();
            // queryList
            sb.Append(space4).Append("<select id=\"queryList\" resultType=\"");
            sb.Append(poClassFullName).AppendLine("\">");
            sb.Append(space8).Append("select * from ").AppendLine(table.TableName);
            sb.Append(space4).AppendLine("</select>");
            sb.AppendLine();
            sb.Append(space4).AppendLine("<!-- 分页查询代码 -->");
            sb.Append(space4).AppendLine("<!--");
            // queryListPage
            sb.Append(space4).Append("<select id=\"queryListPage\" resultType=\"");
            sb.Append(poClassFullName).AppendLine("\">");
            sb.Append(space8).Append("select t.* from ").Append(table.TableName).AppendLine(" t");
            sb.Append(space8).AppendLine("<include refid=\"whereSQL\" />");
            sb.Append(space8).AppendLine("limit ${(pageIndex - 1) * pageSize}, #{pageSize}");
            sb.Append(space4).AppendLine("</select>");
            sb.AppendLine();
            // queryCount
            sb.Append(space4).AppendLine("<select id=\"queryCount\" resultType=\"int\">");
            sb.Append(space8).Append("select count(1) from ").Append(table.TableName).AppendLine(" t");
            sb.Append(space8).AppendLine("<include refid=\"whereSQL\" />");
            sb.Append(space4).AppendLine("</select>");
            sb.AppendLine();
            // whereSQL
            sb.Append(space4).AppendLine("<sql id=\"whereSQL\">");
            sb.Append(space8).AppendLine("<where>");
            foreach (var field in table.Fields)
            {
                string javaType = field.getJavaType();
                sb.Append(space12).Append("<if test=\"").Append(field.FieldName).Append(" != null");
                if ("String".Equals(javaType))
                {
                    sb.Append(" and ").Append(field.FieldName).Append(".trim() != ''");
                }
                sb.AppendLine("\">");
                sb.Append(space16).Append("and t.").Append(field.ColumnName);
                if ("String".Equals(javaType))
                {
                    sb.Append(" like concat(#{").Append(field.FieldName).AppendLine("}, '%')");
                }
                else if ("Date".Equals(javaType))
                {
                    sb.Append(" >= #{").Append(field.FieldName).AppendLine("}");
                }
                else
                {
                    sb.Append(" = #{").Append(field.FieldName).AppendLine("}");
                }
                sb.Append(space12).AppendLine("</if>");
            }
            sb.Append(space8).AppendLine("</where>");
            sb.Append(space4).AppendLine("</sql>");
            sb.AppendLine();
            sb.Append(space4).AppendLine("-->");
            sb.AppendLine();
            sb.AppendLine("</mapper>");
            return new FileVO()
            {
                Body = sb.ToString(),
                FileName = mapperClassName + ".xml"
            };
        }

        private static void fields(StringBuilder sb, TableVo table, string blank, bool hasVar, string var)
        {
            int count = 0;
            int index = -1;
            bool needBlank = true;
            int len = table.Fields.Count;
            var = var == null ? "" : var + ".";
            foreach (var field in table.Fields)
            {
                index++;
                if (field.isAutoIncrement()) continue;
                if (needBlank)
                {
                    sb.Append(blank);
                    needBlank = false;
                }
                if (hasVar)
                {
                    sb.Append("#{").Append(var).Append(field.FieldName).Append("}");
                }
                else
                {
                    sb.Append(field.ColumnName);
                }
                if (index < len - 1 && !(index + 1 < len - 1 && table.Fields[index + 1].isAutoIncrement()))
                {
                    sb.Append(", ");
                }
                count++;
                if (count % fieldCount == 0)
                {
                    needBlank = true;
                    sb.AppendLine();
                }
            }
            if (count % fieldCount != 0)
            {
                sb.AppendLine();
            }
        }

    }
}
