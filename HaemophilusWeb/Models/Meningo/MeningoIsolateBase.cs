using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HaemophilusWeb.Models.Meningo
{
    public class MeningoIsolateBase : IsolateCommon
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MeningoIsolateId { get; set; }

        public MeningoSerogroupAgg Agglutination { get; set; }

        [Display(Name = "ONPG")]
        public TestResult Onpg { get; set; }

        [Display(Name = "gamma-GT")]
        public double? GammaGt { get; set; }

        [Display(Name = "Serogruppen-PCR")]
        public MeningoSerogroupPcr SerogroupPcr { get; set; }
    }
}