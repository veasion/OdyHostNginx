using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OdyHostNginx
{
    interface SwitchHost
    {

        void switchHost(HostConfig host, bool enable);

        void switchHost(List<HostConfig> hosts, bool enable);

    }
}
