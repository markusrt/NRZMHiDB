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
        [RegularExpression(@"\d{5}", ErrorMessage = "Die Postleitzahl muss eine 5-stellige Nummer sein")]
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
        public ClinicalInformation ClinicalInformation { get; set; }

        [Display(Name = "Andere kl. Angaben")]
        public string OtherClinicalInformation { get; set; }

        [Display(Name = "Hib-Impfung")]
        public YesNo HibVaccination { get; set; }

        [Display(Name = "Datum Hib-Impfung")]
        public DateTime? HibVaccinationDate { get; set; }
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

    public enum ClinicalInformation
    {
        [Description("k.A.")]
        NotAvailable,
        [Description("Meningitis")]
        Meningitis,
        [Description("Sepsis")]
        Sepsis,
        [Description("Pneumonie")]
        Pneumonia,
        [Description("Andere")]
        Other
    }

    public enum State
    {
        [Description("Unbekannt")]
        Unknown,
        [Description("Schleswig-Holstein")] SH = 1,
        [Description("Hamburg")] HH = 2,
        [Description("Niedersachsen")] NI = 3,
        [Description("Bremen")] HB = 4,
        [Description("Nordrhein-Westfalen")] NW = 5,
        [Description("Hessen")] HE = 6,
        [Description("Rheinland-Pfalz")] RP = 7,
        [Description("Baden-Württemberg")] BW = 8,
        [Description("Bayern")] BY = 9,
        [Description("Saarland")] SL = 10,
        [Description("Berlin")] BE = 11,
        [Description("Brandenburg")] BB = 12,
        [Description("Mecklenburg-Vorpommern")] MV = 13,
        [Description("Sachsen")] SN = 14,
        [Description("Sachsen-Anhalt")] ST = 15,
        [Description("Thüringen")] TH = 16,
    }
}