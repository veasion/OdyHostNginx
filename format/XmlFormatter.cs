using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;

namespace OdyHostNginx
{
    class XmlFormatter : Formatter
    {

        public string Format(string text)
        {
            try
            {
                return FormatXml(text);
            }
            catch (Exception) { return null; }
        }

        private string FormatXml(string xmlString)
        {
            XmlDocument xd = new XmlDocument();
            xd.LoadXml(xmlString);
            StringBuilder sb = new StringBuilder();
            StringWriter writer = new StringWriter(sb);
            XmlTextWriter xmlTxtWriter = null;
            try
            {
                xmlTxtWriter = new XmlTextWriter(writer);
                xmlTxtWriter.Formatting = Formatting.Indented;
                xmlTxtWriter.Indentation = 1;
                xmlTxtWriter.IndentChar = '\t';
                xd.WriteTo(xmlTxtWriter);
            }
            finally
            {
                if (xmlTxtWriter != null)
                    xmlTxtWriter.Close();
            }
            return sb.ToString();
        }

    }
}
