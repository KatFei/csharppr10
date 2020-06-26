using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SomeProject.Library
{
    [Serializable]
    struct FilePackage
    {
        //string type;
        //string msg;
        string filename;
        byte[] attachment;
        public FilePackage(string file, byte[] attach)
        {
            filename = file;
            attachment = attach;
        }

        public string Filename { get => filename; set => filename = value; }
        public byte[] Attachment { get => attachment; set => attachment = value; }
    }
}
