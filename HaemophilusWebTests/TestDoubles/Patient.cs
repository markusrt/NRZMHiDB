using System.ComponentModel.DataAnnotations;

namespace HaemophilusWeb.TestDoubles
{
    public class Patient
    {
        public string Initials { get; set; }

        [Display(Name = "Alter")]
        public int Age { get; set; }
    }
}