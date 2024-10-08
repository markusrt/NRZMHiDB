﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using HaemophilusWeb.Models;

namespace HaemophilusWeb.ViewModels
{
    public class MergePatientConfirmation : MergePatientRequest
    {
        [Display(Name = "Patient 1")]
        public string PatientOne { get; set; }

        [Display(Name = "Einsendungen Patient 1")]
        public List<string> PatientOneSendings { get; set; }

        [Display(Name = "Patient 2")]
        public string PatientTwo { get; set; }

        [Display(Name = "Einsendungen Patient 2")]
        public List<string> PatientTwoSendings { get; set; }

    }

    public enum MainPatientSelector
    {
        [Description("Patient 1")]
        PatientOne,
        [Description("Patient 2")]
        PatientTwo
    }
}