using System.ComponentModel.DataAnnotations;

namespace HaemophilusWeb.TestDoubles
{
    public class Isolate
    {
        [Display(Name = "Entnahmeort")]
        public string SamplingLocation { get; set; }

        public string Invasive { get; set; }

        [Display(Name = "Der Patient")]
        public Patient Patient { get; set; }
    }
}