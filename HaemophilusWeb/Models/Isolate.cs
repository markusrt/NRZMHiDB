using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using FluentValidation;
using FluentValidation.Attributes;

namespace HaemophilusWeb.Models
{
    public class IsolateDbContext : DbContext
    {
        public DbSet<Isolate> Isolates { get; set; }
    }

    [Validator(typeof (SampleValidator))]
    public class Isolate
    {
        public Isolate()
        {
            ReportingDate = DateTime.Now;
            ReceivingDate = DateTime.Now;
            SamplingDate = DateTime.Now.Subtract(TimeSpan.FromDays(7));
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int HaemophilusId { get; set; }

        [Display(Name = "Einsender")]
        public int SenderId { get; set; }

        [Display(Name = "Labornummer")] //KL-Nr.
        public string LaboratoryNumber { get; set; }

        [Display(Name = "Stammnummer")] //H-Nr.
        public string StemNumber { get; set; }

        [Display(Name = "Eingangsdatum")]
        public DateTime ReceivingDate { get; set; }

        [Display(Name = "Entnahmedatum")]
        public DateTime SamplingDate { get; set; }

        [Display(Name = "Labornummer (Einsender)")]
        public string SenderLaboratory { get; set; }

        [Display(Name = "Material")]
        public Material Material { get; set; }

        [Display(Name = "Invasiv")]
        public bool Invasive { get; set; }

        [Display(Name = "Ergebnis (Einsender)")]
        public string SenderConclusion { get; set; }

        [Display(Name = "Faktorentest")]
        public string FactorTestResults { get; set; }

        [Display(Name = "Ampicillin E-Test (µg/ml)")]
        public float AmpicillinEtest { get; set; }

        [Display(Name = "Amoxicillin/Clavulansäure")]
        public string AmoxicillinToClavulanicAcid { get; set; }

        [Display(Name = "Meropenem E-Test")]
        public float MeropenemEtest { get; set; }

        [Display(Name = "Cefotaxim E-Test")]
        public float CefotaximEtest { get; set; }

        [Display(Name = "Oxidase")]
        public TestResult Oxidase { get; set; }

        [Display(Name = "β-Lactamase")]
        public TestResult BetaLactamase { get; set; }

        [Display(Name = "Agglutination")]
        public TestResult Agglutination { get; set; }

        [Display(Name = "api NH")]
        public TestResult ApiNh { get; set; }

        [Display(Name = "Gen ompP2")]
        public TestResult OmpP2 { get; set; }

        [Display(Name = "Gen bexA")]
        public TestResult BexA { get; set; }

        [Display(Name = "Serotyp-PCR")]
        public TestResult SerotypePcr { get; set; }

        [Display(Name = "16S rRNA")]
        public TestResult Rrna16S { get; set; }

        [Display(Name = "Beurteilung")]
        public string Evaluation { get; set; }

        [Display(Name = "Bemerkung")]
        public string Comment { get; set; }

        [Display(Name = "Befund am")]
        public DateTime ReportingDate { get; set; }
    }

    public class SampleValidator : AbstractValidator<Isolate>
    {
        public SampleValidator()
        {
            RuleFor(sample => sample.ReceivingDate).GreaterThanOrEqualTo(
                sample => sample.SamplingDate).WithMessage("Das Eingangsdatum muss nach dem Entnahmedatum liegen");
        }
    }
}