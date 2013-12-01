using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HaemophilusWeb.Models
{
    public class Patient
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PatientId { get; set; }

        [Display(Name = "Initialen")]
        [Required]
        [RegularExpression(@"([a-zA-ZäöüÄÖÜ]\.)+")]
        public string Initials { get; set; }

        [Display(Name = "Geburtsdatum")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? BirthDate { get; set; }

        [Display(Name = "Postleitzahl")]
        [RegularExpression(@"\d{5}")]
        public string PostalCode { get; set; }

        [Display(Name = "Geschlecht")]
        [Required]
        public Gender? Gender { get; set; }

        [Display(Name = "Wohnort")]
        public string City { get; set; }

        [Display(Name = "Landkreis")]
        public string County { get; set; }

        [Display(Name = "Bundesland")]
        public State State { get; set; }

        [Display(Name = "Klinische Angaben")]
        public string ClinicalInformation { get; set; }

        [Display(Name = "Hib-Impfung")]
        public YesNo HibVaccination { get; set; }
    }

    public enum YesNo
    {
        [Description("Unbekannt")]
        Unknown=0,
        [Description("Ja")]
        Yes=1,
        [Description("Nein")]
        No=2
    }

    public enum Gender
    {
        [Description("männlich")]
        Male=0,
        [Description("weiblich")]
        Female=1
    }

    public enum State
    {
        [Description("Unbekannt")]
        Unknown,
        [Description("Schleswig-Holstein")] SchleswigHolstein = 1,
        [Description("Hamburg")] Hamburg = 2,
        [Description("Niedersachsen")] LowerSaxony = 3,
        [Description("Bremen")] Bremen = 4,
        [Description("Nordrhein-Westfalen")] NorthRhineWestphalia = 5,
        [Description("Hessen")] Hesse = 6,
        [Description("Rheinland-Pfalz")] RhinelandPalatinate = 7,
        [Description("Baden-Württemberg")] BadenWuerttemberg = 8,
        [Description("Bayern")] Bavaria = 9,
        [Description("Saarland")] Saarland = 10,
        [Description("Berlin")] Berlin = 11,
        [Description("Brandenburg")] Brandenburg = 12,
        [Description("Mecklenburg-Vorpommern")] MecklenburgVorpommern = 13,
        [Description("Sachsen")] Saxony = 14,
        [Description("Sachsen-Anhalt")] SaxonyAnhalt = 15,
        [Description("Thüringen")] Thuringia = 16,
    }
}