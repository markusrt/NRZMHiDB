using System;
using System.ComponentModel.DataAnnotations;
using FluentValidation.Attributes;
using HaemophilusWeb.Validators;

namespace HaemophilusWeb.ViewModels
{
    [Validator(typeof(ConfigurationViewModelValidator))]
    public class ConfigurationViewModel
    {
        [Display(Name = "Unterzeichner")]
        public string ReportSigners { get; set; }

        [Display(Name = "Mitteilung")]
        [DataType(DataType.MultilineText)]
        public string AnnouncementText { get; set; }

        [Display(Name = "Gültig von")]
        public DateTime? AnnouncementStart { get; set; }

        [Display(Name = "Gültig bis")]
        public DateTime? AnnouncementEnd { get; set; }

        [Display(Name = "Gesamtleitung")]
        public string LabDirector { get; set; }

        [Display(Name = "Ärztliche Leitung")]
        public string MedicalDirector { get; set; }

        [Display(Name = "Kontaktdaten")]
        [DataType(DataType.MultilineText)]
        public string Contacts { get; set; }
    }
}
