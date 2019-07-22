using HaemophilusWeb.Models;
using HaemophilusWeb.Models.Meningo;

namespace HaemophilusWeb.Domain
{
    public class StemInterpretationRule
    {
        public YesNo? SendingInvasive { get; set; }
        public Growth GrowthOnBloodAgar { get; set; }
        public Growth GrowthOnMartinLewisAgar { get; set; }
        public TestResult? Oxidase { get; set; }
        public MeningoSerogroupAgg? Agglutination { get; set; }
        public TestResult? Onpg { get; set; }
        public TestResult? GammaGt { get; set;  }
        public MeningoSerogroupPcr? SerogroupPcr { get; set; }
        public UnspecificTestResult? MaldiTof { get; set; }
        public NativeMaterialTestResult? PorAPcr { get; set; }
        public NativeMaterialTestResult? FetAPcr { get; set; }
        public string Identification { get; set; }
        public string Interpretation { get; set; }
        public string[] Typings { get; set; }
    }
}