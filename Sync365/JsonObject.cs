using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sync365
{
    public class JsonObject
    {
        public JsonObject() { }
        public string ObjGuid { get; set; }
        public string ObjStatus { get; set; }
        public string Result { get; set; }
        public string task { get; set; }
        public bool Completed { get; set; }
        public string FolderGuid { get; set; }
        public string O_Package_Unload { get; set; }
        public string user { get; set; }
        public string signature { get; set; }
        public string outputpdf { get; set; }
        public string guid { get; set; }
        public string[] files { get; set; }
        public string chatid { get; set; }
        public string text { get; set; }
    }
}
