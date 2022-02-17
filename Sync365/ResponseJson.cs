using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sync365
{
    public class ResponseJson
    {
        public string SystemName { get; set; }
        public bool Completed { get; set; }
        public string Result { get; set; }
        public string Date { get; set; }
        public string O_Package_Unload { get; set; }
        public List<jObject> Objects { get; set; }
    }

    public class jObject
    {
        public string ObjGuid { get; set; }
        public string ObjGuidExternal { get; set; }
        public string ObjStatus { get; set; }
        public string StatusModifyTime { get; set; }
        public string ObjDefName{ get; set; }

    }
}
