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
        public List<jRemark> Remarks { get; set; }
        public jRZ RZ { get; set; }
        //public ResponseJson RJ { get; set; }
    }

    public class jRemark
    {
        public string Description { get; set; }
        public string Guid { get; set; }
        public jStatus Status { get; set; }
        public List<jFile> Files { get; set; }
        public int ATTR_TechDoc_Version { get; set; }
        public int ATTR_Registry_CycleNum { get; set; }
        public int ATTR_Remark_Num { get; set; }
        public string ATTR_REMARK_TYPE { get; set; }
        public string ATTR_Remark { get; set; }
        public string ATTR_AUTHOR_ZM { get; set; }
        public string ATTR_Remark_Date { get; set; }
        public string ATTR_Answer_Type { get; set; }
        public string ATTR_Answer { get; set; }
        public string ATTR_AUTHOR_ANSWER { get; set; }
        public string ATTR_Answer_Date { get; set; }
    }

    public class jStatus
    {
        public string Description { get; set; }
        public string Sysname { get; set; }
        public string StatusModifyTime { get; set; }
        public string StatusModifyUser { get; set; }
    }

}