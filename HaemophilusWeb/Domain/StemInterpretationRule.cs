using System.Collections.Generic;
using HaemophilusWeb.Models;
using HaemophilusWeb.Models.Meningo;

namespace HaemophilusWeb.Domain
{
    public class StemInterpretationRule
    {
        public List<YesNo?> SendingInvasive { get; set; }
        public Growth GrowthOnBloodAgar { get; set; }
        public List<Growth> GrowthOnMartinLewisAgar { get; set; }
        public TestResult? Oxidase { get; set; }
        public List<MeningoSerogroupAgg> Agglutination { get; set; }
        public TestResult? Onpg { get; set; }
        public List<TestResult> GammaGt { get; set;  }
        public List<MeningoSerogroupPcr> SerogroupPcr { get; set; }
        public UnspecificTestResult? MaldiTof { get; set; }
        public NativeMaterialTestResult? PorAPcr { get; set; }
        public NativeMaterialTestResult? FetAPcr { get; set; }
        public string Identification { get; set; }
        public string[] Report { get; set; }
        public string[] Typings { get; set; }
        public string Comment { get; set; }
        public bool NoMeningococci { get; set; }
    }
}