using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OdyHostNginx
{
    public class ConfigShowParam
    {

        private string title = "Confirm";
        private string button = "确认";
        private List<string> keys;
        private List<string> values;
        private CofirmParamCheck check;
        private CofirmParamChangeListener changeListener;

        public string Title { get => title; set => title = value; }
        public string Button { get => button; set => button = value; }
        public List<string> Keys { get => keys; set => keys = value; }
        public List<string> Values { get => values; set => values = value; }
        public CofirmParamCheck Check { get => check; set => check = value; }
        public CofirmParamChangeListener ChangeListener { get => changeListener; set => changeListener = value; }
    }
}
