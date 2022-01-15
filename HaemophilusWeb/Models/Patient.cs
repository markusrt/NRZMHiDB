using System;
using System.ComponentModel.DataAnnotations;
using FluentValidation.Attributes;
using HaemophilusWeb.Validators;

namespace HaemophilusWeb.Models
{
    [Validator(typeof (PatientValidator))]
    public class Patient : PatientBase
    {
        public Patient()
        {
            HibVaccination = VaccinationStatus.NotStated;
            Therapy = YesNoUnknown.NotStated;
        }

        [Display(Name = "Klinische Angaben")]
        public ClinicalInformation ClinicalInformation { get; set; }

        [Display(Name = "Andere kl. Angaben")]
        public string OtherClinicalInformation { get; set; }

        [Display(Name = "Hib-Impfung")]
        public VaccinationStatus HibVaccination { get; set; }

        [Display(Name = "Datum Hib-Impfung")]
        public DateTime? HibVaccinationDate { get; set; }

        [Display(Name = "Therapie")]
        public YesNoUnknown Therapy { get; set; }

        [Display(Name = "Therapie Details")]
        public string TherapyDetails { get; set; }
    }
}