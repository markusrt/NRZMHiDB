using System.ComponentModel.DataAnnotations;
using HaemophilusWeb.Models;

namespace HaemophilusWeb.Views.Utils
{
    public class SimpleModel
    {
        public string SimpleProperty { get; set; }

        [Required]
        public string RequiredProperty { get; set; }

        public GrowthType? GrowthType { get; set; }

        [Display(Name = "Hib-Impfung")]
        public YesNoUnknown HibVaccination { get; set; }

        [Display(Name = "Klinische Angaben")]
        public ClinicalInformation ClinicalInformation { get; set; }
    }
}