using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HaemophilusWeb.Models.Meningo
{
    public class MeningoIsolateBase : IsolateCommon, INeisseriaIsolateAlleleProperties
    {
        public MeningoIsolateBase() : base(DatabaseType.Meningococci)
        {
        }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MeningoIsolateId { get; set; }

        public MeningoSerogroupAgg Agglutination { get; set; }

        [Display(Name = "16S rDNA")]
        public NativeMaterialTestResult RibosomalRna16S { get; set; }

        [Display(Name = "16S rDNA beste Übereinstimmung")]
        public string RibosomalRna16SBestMatch { get; set; }

        [Display(Name = "ONPG")]
        public TestResult Onpg { get; set; }

        [Display(Name = "gamma-GT")]
        public TestResult GammaGt { get; set; }

        [Display(Name = "Serogruppen-PCR")]
        public MeningoSerogroupPcr SerogroupPcr { get; set; }

        [Display(Name = "Wachstum auf Blutagar")]
        public Growth GrowthOnBloodAgar { get; set; }

        [Display(Name = "Wachstum auf Martin-Lewis-Agar")]
        public Growth GrowthOnMartinLewisAgar { get; set; }

        [Display(Name = "porA-PCR")]
        public NativeMaterialTestResult PorAPcr { get; set; }

        [Display(Name = "PorA-VR1")]
        public string PorAVr1 { get; set; }

        [Display(Name = "PorA-VR2")]
        public string PorAVr2 { get; set; }

        [Display(Name = "fetA-PCR")]
        public NativeMaterialTestResult FetAPcr { get; set; }

        [Display(Name = "FetA-VR")]
        public string FetAVr { get; set; }

        [Display(Name = "rplF")]
        public string RplF { get; set; }

        [Display(Name = "csb-PCR")]
        public NativeMaterialTestResult CsbPcr { get; set; }

        [Display(Name = "csc-PCR")]
        public NativeMaterialTestResult CscPcr { get; set; }

        [Display(Name = "cswy-PCR")]
        public NativeMaterialTestResult CswyPcr { get; set; }

        [Display(Name = "cswy-Allel")]
        public CswyAllel CswyAllele { get; set; }

        [Display(Name = "NHS Real-Time-PCR")]
        public NativeMaterialTestResult RealTimePcr { get; set; }

        [Display(Name = "NHS Real-Time-PCR Auswertung (RIDOM)")]
        public RealTimePcrResult RealTimePcrResult { get; set; }

        [Display(Name = "siaA")]
        public TestResult SiaAGene { get; set; }

        [Display(Name = "ctrA")]
        public TestResult CapsularTransferGene { get; set; }

        [Display(Name = "cnl")]
        public TestResult CapsuleNullLocus { get; set; }

    }
}