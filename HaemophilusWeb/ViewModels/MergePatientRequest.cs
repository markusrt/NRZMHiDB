using System;
using System.ComponentModel.DataAnnotations;
using HaemophilusWeb.Models;

namespace HaemophilusWeb.ViewModels
{
    public class MergePatientRequest
    {
        [Display(Name = "Patienten-Nr. 1")]
        [Required]
        [Range(1, int.MaxValue)]
        public int PatientOneId { get; set; }

        [Display(Name = "Patienten-Nr. 2")]
        [Required]
        [Range(1, int.MaxValue)]
        public int PatientTwoId { get; set; }
    }
}