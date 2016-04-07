using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HaemophilusWeb.Models
{
    public class EucastClinicalBreakpoint
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int EucastClinicalBreakpointId { get; set; }

        [Required]
        [Display(Name = "Antibiotikum")]
        public Antibiotic Antibiotic { get; set; }

        [Required]
        [MaxLength(128)]
        [Display(Name = "Antibiotikum Details")]
        public string AntibioticDetails { get; set; }

        [Required]
        [MaxLength(64)]
        public string Version { get; set; }

        [Required]
        [Display(Name = "Gültig ab")]
        public DateTime ValidFrom { get; set; }

        [Required]
        [Display(Name = "Sensibel <=")]
        public float MicBreakpointSusceptible { get; set; }

        [Required]
        [Display(Name = "Resistent >")]
        public float MicBreakpointResistent { get; set; }
    }
}