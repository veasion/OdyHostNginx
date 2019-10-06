using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OdyHostNginx
{
    public class KeyValue
    {

        private string key;
        private string value;

        public string Key { get => key; set => key = value; }
        public string Value { get => value; set => this.value = value; }

        public static ObservableCollection<KeyValue> list(Dictionary<string, string> dic)
        {
            if (dic == null) return null;
            ObservableCollection<KeyValue> list = new ObservableCollection<KeyValue>();
            foreach (var k in dic.Keys)
            {
                list.Add(new KeyValue()
                {
                    key = k,
                    value = dic[k]
                });
            }
            return list;
        }

    }
}
