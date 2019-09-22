using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OdyHostNginx
{
    interface SwitchHost
    {

        void switchHost(string domain, string ip, bool enable);

        void switchHost(string[] domain, string[] ip, bool enable);

    }
}
