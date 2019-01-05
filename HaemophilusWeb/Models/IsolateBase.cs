using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HaemophilusWeb.Models
{
    public class IsolateBase : IsolateCommon
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IsolateId { get; set; }

        [Display(Name = "Faktoren-test")]
        public FactorTest FactorTest { get; set; }

        public SerotypeAgg Agglutination { get; set; }

        [Display(Name = "ß-Laktamase")]
        public TestResult BetaLactamase { get; set; }

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

        [Display(Name = "ftsI")]
        public UnspecificTestResult Ftsi { get; set; }

        [Display(Name = "ftsI Beurteilung")]
        public string FtsiEvaluation1 { get; set; }

        [Display(Name = "ftsI Beurteilung 2")]
        public string FtsiEvaluation2 { get; set; }

        [Display(Name = "ftsI Beurteilung 3")]
        public string FtsiEvaluation3 { get; set; }

        [Display(Name = "MLST")]
        public UnspecificOrNoTestResult Mlst { get; set; }

        [Display(Name = "MLST Sequenztyp")]
        public string MlstSequenceType { get; set; }

        [Display(Name = "Beurteilung")]
        public Evaluation Evaluation { get; set; }
    }
}