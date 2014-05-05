using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Script.Serialization;

namespace HaemophilusWeb.Models
{
    public class Isolate
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IsolateId { get; set; }

        [Display(Name = "Stammnummer")]
        public int? StemNumber { get; set; }

        [Display(Name = "Einsendung")]
        [Key]
        [ForeignKey("Sending")]
        public int SendingId { get; set; }

        [ScriptIgnore]
        public Sending Sending { get; set; }

        public int YearlySequentialIsolateNumber { get; set; }

        public int Year { get; set; }

        [Display(Name = "Labornummer")]
        public string LaboratoryNumber
        {
            get { return string.Format("{0}/{1}", YearlySequentialIsolateNumber, Year - 2000); }
        }

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
        public TestResult OuterMembraneProteinP6 { get; set; }

        [Display(Name = "bexA")]
        public TestResult BexA { get; set; }

        [Display(Name = "Serotyp-PCR")]
        public Serotype SerotypePcr { get; set; }

        [Display(Name = "16S rRNA")]
        public TestResult RibosomalRna16S { get; set; }

        [Display(Name = "api NH")]
        public TestResult ApiNh { get; set; }

        public virtual ICollection<EpsilometerTest> EpsilometerTests { get; set; }
    }
}