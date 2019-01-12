using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HaemophilusWeb.Models.Meningo
{
    public class MeningoIsolateBase : IsolateCommon
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MeningoIsolateId { get; set; }

        public MeningoSerotypeAgg Agglutination { get; set; }

        [Display(Name = "ONPG")]
        public TestResult Onpg { get; set; }

        [Display(Name = "gamma-GT")]
        public double? GammaGt { get; set; }
    }
}