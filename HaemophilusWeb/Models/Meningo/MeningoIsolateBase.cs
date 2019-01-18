using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HaemophilusWeb.Models.Meningo
{
    public class MeningoIsolateBase : IsolateCommon, INeisseriaIsolateAlleleProperties
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MeningoIsolateId { get; set; }

        public MeningoSerogroupAgg Agglutination { get; set; }

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

        [Display(Name = "PorA-VR1")]
        public string PorAVr1 { get; set; }

        [Display(Name = "PorA-VR2")]
        public string PorAVr2 { get; set; }

        [Display(Name = "FetA-VR")]
        public string FetAVr { get; set; }

        [Display(Name = "rplF")]
        public string RplF { get; set; }
    }
}