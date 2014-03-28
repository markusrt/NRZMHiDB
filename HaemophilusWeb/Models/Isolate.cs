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
        [Display(Name = "Stammnummer")]
        public int IsolateId { get; set; }

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

        [Description("Faktoren-test")]
        public FactorTest? FactorTest { get; set; }

        public Serotype? Agglutination { get; set; }

        //Ampicillin	E-Test

        [Description("ß-Laktamase")]
        public TestResult? BetaLactamase { get; set; }

        //Oxidase	Positiv, Negativ, n.d.
        public TestResult? Oxidase { get; set; }

        //Amoxicillin/Clavulansäure	E-Test
        //Cefotaxim	E-Test
        //Meropenem	E-Test

        [Description("opm-P2")]
        public TestResult? OuterMembraneProteinP2 { get; set; }

        [Description("fucK")]
        public TestResult FuculoKinase { get; set; }

        [Description("opm-P6")]
        public TestResult OuterMembraneProteinP6 { get; set; }

        [Description("bexA")]
        public TestResult? BexA { get; set; }

        [Description("Serotyp-PCR")]
        public Serotype? SerotypePcr { get; set; }

        [Description("16S rRNA")]
        public TestResult RibosomalRna16S { get; set; }

        [Description("api NH")]
        public TestResult? ApiNh { get; set; }

        public virtual ICollection<EpsilometerTest> EpsilometerTests { get; set; }
    }
}