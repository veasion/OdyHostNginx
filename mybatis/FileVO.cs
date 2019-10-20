using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OdyHostNginx
{
    class FileVO
    {
        private string body;
        private string fileName;

        public string Body { get => body; set => body = value; }
        public string FileName { get => fileName; set => fileName = value; }

    }
}
