using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sync365
{
    public class jRZ
    {
        public string Description { get; set; }
        public string Guid { get; set; }
        public jStatus Status { get; set; }
        public List<jFile> Files { get; set; }
        public List<jExpert> Experts { get; set; }
        public string ATTR_REGYSTRY_CREATION_DATE { get; set; }
        public string ATTR_TechDoc_For_Observation { get; set; }
        public string TD_External_Guid { get; set; }
        public string ATTR_NAME_REGISTRY { get; set; }
        public int ATTR_Registry_Num { get; set; }
        public string ATTR_REGYSTRY_COMPLETE_THE_STAGE_BEFORE { get; set; }
        public string ATTR_Registry_UserInitiated { get; set; }
        public string ATTR_REGISTRY_LAUNCH_A_PLAN { get; set; }
        public string ATTR_REGISTRY_LAUNCHED_BY_THE_FACT { get; set; }
        public string ATTR_REGISTRY_PLAN_DATE_OF_FINISH { get; set; }
        public string ATTR_Registry_UserFinished { get; set; }
        public string ATTR_REGISTRY_FACT_DATE_OF_FINISH { get; set; }
    }

    public class jFile
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public string Hash { get; set; }
    }
    public class jExpert
    {
        public string Description { get; set; }
        public string Sysname { get; set; }
        public bool Finished { get; set; }
    }
}