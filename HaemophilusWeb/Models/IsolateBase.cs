using System;
using System.ComponentModel.DataAnnotations;
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

        public SerotypeAgg Agglutination { get; set; }

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
        public SerotypePcr SerotypePcr { get; set; }

        [Display(Name = "16S rRNA")]
        public UnspecificTestResult RibosomalRna16S { get; set; }

        [Display(Name = "16S rRNA beste Übereinstimmung")]
        public string RibosomalRna16SBestMatch { get; set; }

        [Display(Name = "16S rRNA Übereinstimmung")]
        public double? RibosomalRna16SMatchInPercent { get; set; }

        [Display(Name = "api NH")]
        public UnspecificTestResult ApiNh { get; set; }

        [Display(Name = "api NH beste Übereinstimmung")]
        public string ApiNhBestMatch { get; set; }

        [Display(Name = "api NH Übereinstimmung")]
        public double? ApiNhMatchInPercent { get; set; }

        [Display(Name = "MALDI-TOF")]
        public UnspecificTestResult MaldiTof { get; set; }

        [Display(Name = "MALDI-TOF beste Übereinstimmung")]
        public string MaldiTofBestMatch { get; set; }

        [Display(Name = "MALDI-TOF Übereinstimmung")]
        public double? MaldiTofMatchConfidence { get; set; }

        [Display(Name = "ftsI")]
        public UnspecificTestResult Ftsi { get; set; }

        [Display(Name = "ftsI Beurteilung")]
        public string FtsiEvaluation1 { get; set; }

        [Display(Name = "ftsI Beurteilung 1")]
        public string FtsiEvaluation2 { get; set; }

        [Display(Name = "MLST")]
        public UnspecificOrNoTestResult Mlst { get; set; }

        [Display(Name = "MLST Sequenztyp")]
        public string MlstSequenceType { get; set; }

        [Display(Name = "Beurteilung")]
        public Evaluation Evaluation { get; set; }

        [Display(Name = "Befund am")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}", ApplyFormatInEditMode = true)]
        public DateTime? ReportDate { get; set; }

        [Display(Name = "Bemerkung")]
        public string Remark { get; set; }
    }
}