using System.ComponentModel.DataAnnotations;

namespace HaemophilusWeb.Models
{
    public class RkiExportRecord
    {
        [Display(Name = "Stammnummer")]
        public int? StemNumber { get; set; }

        [Display(Name = "Eingangsdatum")]
        public string ReceivingDate { get; set; }

        [Display(Name = "Entnahmedatum")]
        public string SamplingDate { get; set; }

        [Display(Name = "Material")]
        public string SamplingLocation { get; set; }

        [Display(Name = "Geburtsmonat")]
        public int Birthmonth { get; set; }

        [Display(Name = "Geburtsjahr")]
        public int Birthyear { get; set; }

        [Display(Name = "Geschlecht")]
        public string Gender { get; set; }

        [Display(Name = "Landkreis")]
        public string County { get; set; }

        [Display(Name = "Kreisnr")]
        public string CountyNumber { get; set; }

        [Display(Name = "Bundesland")]
        public string State { get; set; }

        [Display(Name = "Kennziffer")]
        public string StateNumber { get; set; }

        [Display(Name = "Hib-Impfung")]
        public string HibVaccination { get; set; }

        [Display(Name = "ß-Laktamase")]
        public string BetaLactamase { get; set; }

        [Display(Name = "Ampicillin MHK")]
        public double? AmpicillinMeasurement { get; set; }

        [Display(Name = "Ampicillin Bewertung")]
        public string AmpicillinEpsilometerTestResult { get; set; }

        [Display(Name = "Amoxicillin/Clavulansäure MHK")]
        public double? AmoxicillinClavulanateMeasurement { get; set; }

        [Display(Name = "Bewertung Amoxicillin/Clavulansäure")]
        public string AmoxicillinClavulanateEpsilometerTestResult { get; set; }

        [Display(Name = "Serotyp")]
        public string Evaluation { get; set; }

        [Display(Name = "Einsendernummer")]
        public int SenderId { get; set; }
    }
}