using System.Collections.Generic;
using HaemophilusWeb.Models;
using HaemophilusWeb.Models.Meningo;

namespace HaemophilusWeb.Domain
{
    public class NativeMaterialInterpretationRule
    {
        public List<NativeMaterialTestResult> CsbPcr { get; set; }
        public List<NativeMaterialTestResult> CscPcr { get; set; }
        public List<NativeMaterialTestResult> CswyPcr { get; set; }
        public CswyAllel CswyAllele { get; set; }
        public List<NativeMaterialTestResult> PorAPcr { get; set; }
        public List<NativeMaterialTestResult> FetAPcr { get; set; }
        public List<NativeMaterialTestResult> RibosomalRna16S { get; set; }
        public string RibosomalRna16SBestMatch { get; set; }
        public List<NativeMaterialTestResult> RealTimePcr { get; set; }
        public RealTimePcrResult RealTimePcrResult { get; set; }
        public string MolecularTyping { get; set; }
        public string[] Report { get; set; }
        public string[] Typings { get; set; }
        public string Comment { get; set; }
    }
}