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

    public class jRZ
    {
        /// <summary>
        /// Îïèñàíèå îáúåêòà
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// GUID Ðååñòðà çàìå÷àíèé
        /// </summary>
        public string Guid { get; set; }

        /// <summary>
        /// Ñòàòóñ Ðååñòðà çàìå÷àíèé
        /// </summary>
        public jStatus Status { get; set; }

        /// <summary>
        /// Ñïèñîê ôàéëîâ çàìå÷àíèÿ
        /// </summary>
        public List<jFile> Files { get; set; }

        /// <summary>
        /// Ýêñïåðòíàÿ ãðóïïà Ðååñòðà çàìå÷àíèé
        /// </summary>
        public List<jExpert> Experts { get; set; }

        /// <summary>
        /// Àòðèáóò - Ñîçäàí
        /// </summary>
        public string ATTR_REGYSTRY_CREATION_DATE { get; set; }

        /// <summary>
        /// Àòðèáóò - Äîêóìåíòàöèÿ (GUID â TDM365)
        /// </summary>
        public string ATTR_TechDoc_For_Observation { get; set; }

        /// <summary>
        /// Àòðèáóò - Äîêóìåíòàöèÿ (GUID âî âíåøíåé ñèñòåìå)
        /// </summary>
        public string TD_External_Guid { get; set; }

        /// <summary>
        /// Àòðèáóò - Íàèìåíîâàíèå
        /// </summary>
        public string ATTR_NAME_REGISTRY { get; set; }

        /// <summary>
        /// Àòðèáóò - Íîìåð
        /// </summary>
        public int ATTR_Registry_Num { get; set; }

        /// <summary>
        /// Àòðèáóò - Çàâåðøèòü ýòàï äî
        /// </summary>
        public string ATTR_REGYSTRY_COMPLETE_THE_STAGE_BEFORE { get; set; }

        /// <summary>
        /// Àòðèáóò - Èíèöèèðîâàë
        /// </summary>
        public string ATTR_Registry_UserInitiated { get; set; }

        /// <summary>
        /// Àòðèáóò - Çàïóñòèòü (ïëàí)
        /// </summary>
        public string ATTR_REGISTRY_LAUNCH_A_PLAN { get; set; }

        /// <summary>
        /// Àòðèáóò - Çàïóùåí (ôàêò)
        /// </summary>
        public string ATTR_REGISTRY_LAUNCHED_BY_THE_FACT { get; set; }

        /// <summary>
        /// Àòðèáóò - Çàâåðøèòü (ïëàí)
        /// </summary>
        public string ATTR_REGISTRY_PLAN_DATE_OF_FINISH { get; set; }

        /// <summary>
        /// Àòðèáóò - Çàâåðøèë
        /// </summary>
        public string ATTR_Registry_UserFinished { get; set; }

        /// <summary>
        /// Àòðèáóò - Çàâåðøåí
        /// </summary>
        public string ATTR_REGISTRY_FACT_DATE_OF_FINISH { get; set; }
    }

    public class jRemark
    {
        /// <summary>
        /// Îïèñàíèå îáúåêòà
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// GUID çàìå÷àíèÿ
        /// </summary>
        public string Guid { get; set; }

        /// <summary>
        /// Ñòàòóñ Ðååñòðà çàìå÷àíèé
        /// </summary>
        public jStatus Status { get; set; }

        /// <summary>
        /// Ñïèñîê ôàéëîâ çàìå÷àíèÿ
        /// </summary>
        public List<jFile> Files { get; set; }

        /// <summary>
        /// Àòðèáóò - Âåðñèÿ
        /// </summary>
        public int ATTR_TechDoc_Version { get; set; }

        /// <summary>
        /// Àòðèáóò - Öèêë
        /// </summary>
        public int ATTR_Registry_CycleNum { get; set; }

        /// <summary>
        /// Àòðèáóò - Íîìåð çàìå÷àíèÿ
        /// </summary>
        public int ATTR_Remark_Num { get; set; }

        /// <summary>
        /// Àòðèáóò - Çàìå÷àíèå
        /// </summary>
        public string ATTR_REMARK_TYPE { get; set; }

        /// <summary>
        /// Àòðèáóò - Îïèñàíèå
        /// </summary>
        public string ATTR_Remark { get; set; }

        /// <summary>
        /// Àòðèáóò - Àâòîð çàìå÷àíèÿ
        /// </summary>
        public string ATTR_AUTHOR_ZM { get; set; }

        /// <summary>
        /// Àòðèáóò - Äàòà çàìå÷àíèÿ
        /// </summary>
        public string ATTR_Remark_Date { get; set; }

        /// <summary>
        /// Àòðèáóò - Îòâåò
        /// </summary>
        public string ATTR_Answer_Type { get; set; }

        /// <summary>
        /// Àòðèáóò - Îáîñíîâàíèå
        /// </summary>
        public string ATTR_Answer { get; set; }

        public string ATTR_AUTHOR_ANSWER { get; set; }

        public string ATTR_Answer_Date { get; set; }
    }

    public class jFile
    {
        public string Name { get; set; }
        public string Path { get; set; }
             /// <summary>
        /// Èìÿ ôàéëà
        /// </summary>
   public string Hash { get; set; }
    }

    public class jStatus
    {
     
        /// <summary>
        /// Md5 õýø ôàéëà
        /// </summary>
   public string Description { get; set; }

        public string Sysname { get; set; }

        public string StatusModifyTime { get; set; }

        public string StatusModifyUser { get; set; }
    }

    public class jExpert
    {
        public string Description { get; set; }

        public string Sysname { get; set; }

        public bool Finished { get; set; }
    }

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
    }
}