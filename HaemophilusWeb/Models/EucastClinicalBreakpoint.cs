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
        [MaxLength(128)]
        public string Penicillin { get; set; }

        [Required]
        [MaxLength(64)]
        public string Version { get; set; }

        [Required]
        public DateTime ValidFrom { get; set; }

        [Required]
        public float MicBreakpointSusceptible { get; set; }

        [Required]
        public float MicBreakpointResistent { get; set; }
    }
}