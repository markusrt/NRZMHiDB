using System.Collections.Generic;
using HaemophilusWeb.Models;
using HaemophilusWeb.Models.Meningo;

namespace HaemophilusWeb.Domain
{
    public class NativeMaterialInterpretationRule
    {
        public List<NativeMaterialTestResult> RibosomalRna16S { get; set; }
        public string RibosomalRna16SBestMatch { get; set; }
        public List<NativeMaterialTestResult> RealTimePcr { get; set; }
        public RealTimePcrResult RealTimePcrResult { get; set; }
        public TestResult? FuculoKinase { get; set; }
        public string[] Report { get; set; }
        public string[] Typings { get; set; }
        public string Comment { get; set; }
        public bool NoHaemophilusInfluenzae { get; set; }
        public string Remark { get; set; }
    }
}