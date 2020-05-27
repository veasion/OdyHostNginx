using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OdyHostNginx
{
    class ConfigDialogData
    {

        public static string projectName;
        public static string envName;
        public static string path;

        public static List<HostConfig> hosts;

        public static bool success;

        public static HttpPacketClient httpPacketClient = new HttpPacketClient();

        public static bool modifyResponse = false;

    }
}
