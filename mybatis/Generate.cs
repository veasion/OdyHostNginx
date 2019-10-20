using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OdyHostNginx
{
    class Generate
    {

        private string tablePrefix;
        private string poSuffix = "PO";
        private string voSuffix = "VO";
        private string mapperSuffix = "Mapper";
        private string poPackage;
        private string voPackage;
        private string mapperPackage;

        public string TablePrefix { get => tablePrefix; set => tablePrefix = value; }
        public string PoSuffix { get { return poSuffix != null ? poSuffix : ""; } set => poSuffix = value; }
        public string VoSuffix { get { return voSuffix != null ? voSuffix : ""; } set => voSuffix = value; }
        public string MapperSuffix { get { return mapperSuffix != null ? mapperSuffix : "Mapper"; } set => mapperSuffix = value; }
        public string PoPackage { get => poPackage; set => poPackage = value; }
        public string VoPackage { get => voPackage; set => voPackage = value; }
        public string MapperPackage { get => mapperPackage; set => mapperPackage = value; }

        public static Generate GetGenerate(string db)
        {

            Generate generate = new Generate();
            if (!StringHelper.isEmpty(db))
            {
                db = db.Trim().ToLower();
                generate.tablePrefix = "";
                generate.PoPackage = "com.odianyun." + db + ".model.po";
                generate.VoPackage = "com.odianyun." + db + ".model.vo";
                generate.MapperPackage = "com.odianyun." + db + ".mapper";
            }
            return generate;
        }

    }
}
