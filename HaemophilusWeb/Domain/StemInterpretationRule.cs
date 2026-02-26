using HaemophilusWeb.Models;
using HaemophilusWeb.Models.Meningo;
using System.Collections.Generic;

namespace HaemophilusWeb.Domain
{
    public class StemInterpretationRule
    {
        public List<YesNo?> SendingInvasive { get; set; }
        public TestResult? Oxidase { get; set; }
        public List<SerotypeAgg> Agglutination { get; set; }
        public List<SerotypePcr> SerotypePcr { get; set; }
        public TestResult? BexA { get; set; }
        public TestResult? BetaLactamase { get; set; }
        public UnspecificTestResult? MaldiTofVitek { get; set; }
        public UnspecificTestResult? MaldiTofBiotyper { get; set; }
        public UnspecificTestResult? ApiNh { get; set; }
        public YesNoOptional? Growth { get; set; }
        public Evaluation? Evaluation { get; set; }
        public string Identification { get; set; }
        public string[] Report { get; set; }
        public string[] Typings { get; set; }
        public string Comment { get; set; }
        public bool Preliminary { get; set; }
    }
}