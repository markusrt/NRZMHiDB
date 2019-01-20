using System;
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

        [Display(Name = "Gültig für")]
        public DatabaseType ValidFor { get; set; }

        [Display(Name = "Gültig ab")]
        public DateTime? ValidFrom { get; set; }


        [Display(Name = "Sensibel <=")]
        public float? MicBreakpointSusceptible { get; set; }

        [Display(Name = "Resistent >")]
        public float? MicBreakpointResistent { get; set; }


        [NotMapped]
        public string Title
        {
            get
            {
                if (NoEucastAvailable)
                {
                    return string.Format("{0} - ohne EUCAST Grenzwerte", AntibioticDetails);
                }
                return string.Format("{0} - v{1} vom {2:dd. MMM. yy}", AntibioticDetails, Version,
                    ValidFrom);
            }
        }

        [NotMapped]
        public bool NoEucastAvailable
        {
            get
            {
                return Version == null && ValidFrom == null && MicBreakpointResistent == null && MicBreakpointSusceptible == null;
            }
        }
    }
}