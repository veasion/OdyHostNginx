using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OdyHostNginx
{
    class Result
    {

        private List<FileVO> pos;
        private List<FileVO> vos;
        private List<FileVO> xmls;
        private List<FileVO> mappers;

        internal List<FileVO> Pos { get => pos; set => pos = value; }
        internal List<FileVO> Vos { get => vos; set => vos = value; }
        internal List<FileVO> Xmls { get => xmls; set => xmls = value; }
        internal List<FileVO> Mappers { get => mappers; set => mappers = value; }

        public void saveTo(string dir)
        {
            string tmp = dir + "\\po";
            FileHelper.mkdir(tmp);
            saveFile(tmp, Pos);

            tmp = dir + "\\vo";
            FileHelper.mkdir(tmp);
            saveFile(tmp, Vos);

            tmp = dir + "\\mapper";
            FileHelper.mkdir(tmp);
            saveFile(tmp, Xmls);
            saveFile(tmp, Mappers);
        }

        private void saveFile(string dir, List<FileVO> files)
        {
            if (files != null)
            {
                foreach (var file in files)
                {
                    FileHelper.writeFile(dir + "\\" + file.FileName, file.Body);
                }
            }
        }

    }
}
