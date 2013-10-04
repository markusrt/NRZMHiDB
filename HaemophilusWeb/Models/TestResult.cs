using System.ComponentModel.DataAnnotations;

namespace HaemophilusWeb.Models
{
    public enum TestResult  
    {
        [Display(Name = "Positiv")]
        Positive,
        [Display(Name = "Negativ")]
        Negative
    }
}