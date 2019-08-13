using HaemophilusWeb.Models;
using HaemophilusWeb.Models.Meningo;

namespace HaemophilusWeb.Domain
{
    public class NativeMaterialInterpretationRule
    {
        public NativeMaterialTestResult CsbPcr { get; set; }
        public NativeMaterialTestResult CscPcr { get; set; }
        public NativeMaterialTestResult CswyPcr { get; set; }
        public CswyAllel CswyAllele { get; set; }
        public NativeMaterialTestResult PorAPcr { get; set; }
        public NativeMaterialTestResult FetAPcr { get; set; }
        public NativeMaterialTestResult RibosomalRna16S { get; set; }
        public string RibosomalRna16SBestMatch { get; set; }
        public NativeMaterialTestResult RealTimePcr { get; set; }
        public RealTimePcrResult RealTimePcrResult { get; set; }
        public string MolecularTyping { get; set; }
        public string Interpretation { get; set; }
        public string InterpretationDisclaimer { get; set; }
        public string[] Typings { get; set; }
    }
}