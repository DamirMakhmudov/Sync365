using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sync365
{
    public class ResponseJson_
    {
        public string SystemName { get; set; }
        public bool Completed { get; set; }
        public string Result { get; set; }
        public string Date { get; set; }
        public string O_Package_Unload { get; set; }
        public List<jObject_d> Objects { get; set; }
    }

    public class jObject_d
    {
        public string ObjGuid { get; set; }
        public string ObjGuidExternal { get; set; }
        public string ObjStatus { get; set; }
        public string StatusModifyTime { get; set; }
        public string ObjDefName{ get; set; }
        public List<jAttr_> Attrs { get; set; }
    }
    public class jAttr_
    {
        public string Description { get; set; }
        public string SysName { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }
    }
}
