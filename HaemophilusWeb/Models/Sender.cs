using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HaemophilusWeb.Models
{
    public class Sender
    {

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SenderId { get; set; }

        [Display(Name = "Name")]
        [Required]
        public string Name { get; set; }

        [Display(Name = "Art")]
        public string Type { get; set; }

        [Display(Name = "Postleitzahl")]
        [Required]
        public string PostalCode { get; set; }

        [Display(Name = "Ort")]
        [Required]
        public string City { get; set; }

        [Display(Name = "Straße")]
        [Required]
        public string Street { get; set; }

        [Display(Name = "Kontakt")]
        public string ContactInfo { get; set; }
    }
}