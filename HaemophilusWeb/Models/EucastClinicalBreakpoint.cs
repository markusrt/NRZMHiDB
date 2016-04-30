﻿using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FluentValidation.Attributes;
using HaemophilusWeb.Validators;

namespace HaemophilusWeb.Models
{
    [Validator(typeof(EucastClinicalBreakpointValidator))]
    public class EucastClinicalBreakpoint
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int EucastClinicalBreakpointId { get; set; }

        [Display(Name = "Antibiotikum")]
        public Antibiotic Antibiotic { get; set; }

        [MaxLength(128)]
        [Display(Name = "Antibiotikum Details")]
        public string AntibioticDetails { get; set; }

        [MaxLength(64)]
        public string Version { get; set; }

        [Display(Name = "Gültig ab")]
        public DateTime? ValidFrom { get; set; }

        [Display(Name = "Sensibel <=")]
        public float? MicBreakpointSusceptible { get; set; }

        [Display(Name = "Resistent >")]
        public float? MicBreakpointResistent { get; set; }
    }
}