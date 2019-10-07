using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OdyHostNginx
{
    public class TracesInfo
    {
        private string id;
        private string pid;
        private string name;
        private long? timestamp;
        private long? duration;
        private string serviceName;
        private string clientName;
        private Dictionary<string, string> details;

        private List<TracesInfo> children;

        public string Id { get => id; set => id = value; }
        public string Pid { get => pid; set => pid = value; }
        public string Name
        {
            get
            {
                return StringHelper.replaceMultipleBlank(StringHelper.replaceLine(name, " "));
            }
            set => name = value;
        }
        public long? Timestamp { get => timestamp; set => timestamp = value; }
        public long? Duration { get => duration; set => duration = value; }
        public string ServiceName { get => serviceName; set => serviceName = value; }
        public string ClientName { get => clientName; set => clientName = value; }
        public Dictionary<string, string> Details { get => details; set => details = value; }
        public List<TracesInfo> Children { get => children; set => children = value; }

        public string Pool()
        {
            return serviceName != null ? serviceName : clientName;
        }

    }
}
