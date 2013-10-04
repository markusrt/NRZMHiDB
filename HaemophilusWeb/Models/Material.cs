using System.ComponentModel.DataAnnotations;

namespace HaemophilusWeb.Models
{
    public enum Material
    {
        [Display(Name = "Blut")]
        Blood,
        [Display(Name = "Liquor")]
        Liquor
    }
}