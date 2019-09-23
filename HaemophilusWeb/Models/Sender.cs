using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HaemophilusWeb.Models
{
    public class Sender
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "Einsendernummer")]
        public int SenderId { get; set; }

        [Display(Name = "Name")]
        [Required]
        public string Name { get; set; }

        [Display(Name = "Abteilung")]
        public string Department { get; set; }

        [Display(Name = "Straße")]
        public string StreetWithNumber { get; set; }

        [Display(Name = "Postleitzahl")]
        public string PostalCode { get; set; }

        [Display(Name = "Ort")]
        public string City { get; set; }

        [Display(Name = "Telefon #1")]
        [Required]
        [Phone]
        public string Phone1 { get; set; }

        [Display(Name = "Telefon #2")]
        [Phone]
        public string Phone2 { get; set; }

        [Display(Name = "Fax")]
        [Phone]
        public string Fax { get; set; }

        [Display(Name = "E-Mail")]
        [EmailAddress]
        public string Email { get; set; }

        [Display(Name = "Bemerkung")]
        public string Remark { get; set; }

        [Display(Name = "Gelöscht")]
        public bool Deleted { get; set; }

        //TODO Implement Country
        //[Display(Name = "Land")]
        //public string Country
    }
}