﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HaemophilusWeb.Models
{
    public class IsolateBase
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IsolateId { get; set; }

        [Display(Name = "Stammnummer")]
        public int? StemNumber { get; set; }

        [Display(Name = "Faktoren-test")]
        public FactorTest FactorTest { get; set; }

        public Serotype Agglutination { get; set; }

        [Display(Name = "ß-Laktamase")]
        public TestResult BetaLactamase { get; set; }

        public TestResult Oxidase { get; set; }

        [Display(Name = "ompP2")]
        public TestResult OuterMembraneProteinP2 { get; set; }

        [Display(Name = "fucK")]
        public TestResult FuculoKinase { get; set; }

        [Display(Name = "ompP6")]
        public SpecificTestResult OuterMembraneProteinP6 { get; set; }

        [Display(Name = "bexA")]
        public TestResult BexA { get; set; }

        [Display(Name = "Serotyp-PCR")]
        public Serotype SerotypePcr { get; set; }

        [Display(Name = "16S rRNA")]
        public UnspecificTestResult RibosomalRna16S { get; set; }

        [Display(Name = "16S rRNA Übereinstimmung")]
        public double? RibosomalRna16SMatchInPercent { get; set; }

        [Display(Name = "api NH")]
        public UnspecificTestResult ApiNh { get; set; }

        [Display(Name = "api NH Übereinstimmung")]
        public double? ApiNhMatchInPercent { get; set; }

        [Display(Name = "MALDI-TOF")]
        public UnspecificTestResult MaldiTof { get; set; }

        [Display(Name = "MALDI-TOF Übereinstimmung")]
        public double? MaldiTofMatchConfidence { get; set; }
    }
}