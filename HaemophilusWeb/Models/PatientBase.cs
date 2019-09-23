using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using HaemophilusWeb.Controllers;

namespace HaemophilusWeb.Models
{
    public class PatientBase
    {
        private string initials;
        private string country;

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
                if (initials != null)
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

        [Display(Name = "Land")]
        public string Country
        {
            get => country;
            set => country = value ?? GeonamesController.DefaultCountryIsoAlpha3;
        }

        [Display(Name = "Bundesland")]
        public State State { get; set; }
    }
}