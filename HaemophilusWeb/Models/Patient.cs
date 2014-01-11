using System;
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
        [RegularExpression(Validations.PostalCodeValidation, ErrorMessage = Validations.PostalCodeValidationError)]
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
        public YesNoUnknown HibVaccination { get; set; }

        [Display(Name = "Datum Hib-Impfung")]
        public DateTime? HibVaccinationDate { get; set; }
    }
}