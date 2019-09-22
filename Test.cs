using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OdyHostNginx
{
    class Test
    {
        public static void test()
        {
            List<ProjectConfig> projects = OdyConfigHelper.loadConfig(@"C:\Users\Veasion\Desktop\test");
            OdyConfigHelper.writeConfig(projects, @"C:\Users\Veasion\Desktop\test2");
        }

    }
}
