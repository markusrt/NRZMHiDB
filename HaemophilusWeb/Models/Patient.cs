using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using FluentValidation.Attributes;
using HaemophilusWeb.Validators;

namespace HaemophilusWeb.Models
{
    [Validator(typeof (PatientValidator))]
    public class Patient
    {
        private string initials;

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PatientId { get; set; }

        [Display(Name = "Initialen")]
        public string Initials
        {
            get { return initials; }
            set
            {
                initials = value;
                if (initials!=null)
                {
                    initials = initials.ToUpper(CultureInfo.InvariantCulture);
                }
                
            }
        }

        [Display(Name = "Geburtsdatum")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? BirthDate { get; set; }

        [Display(Name = "Postleitzahl")]
        public string PostalCode { get; set; }

        [Display(Name = "Geschlecht")]
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