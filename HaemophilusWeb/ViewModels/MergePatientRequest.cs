using System;
using System.ComponentModel.DataAnnotations;
using FluentValidation.Attributes;
using HaemophilusWeb.Validators;

namespace HaemophilusWeb.ViewModels
{
    [Validator(typeof (MergePatientRequestValidator))]
    public class MergePatientRequest
    {
        [Display(Name = "Patienten-Nr. 1")]
        public int PatientOneId { get; set; }

        [Display(Name = "Patienten-Nr. 2")]
        public int PatientTwoId { get; set; }

        [Display(Name = "Zusammenfügen zu")]
        public MainPatientSelector MainPatient { get; set; } 

        public bool Confirmation { get; set; }
    }
}