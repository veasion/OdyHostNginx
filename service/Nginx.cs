using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OdyHostNginx
{
    interface Nginx
    {

        void restart();

        void stop();

        void include(List<string> list);

        void reset();

    }
}
