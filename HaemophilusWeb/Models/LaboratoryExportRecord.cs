using System.ComponentModel.DataAnnotations;

namespace HaemophilusWeb.Models
{
    public class LaboratoryExportRecord : ExportRecord
    {
        [Display(Name = "Patientenalter bei Entnahme")]
        public int PatientAgeAtSampling { get; set; }
    }
}