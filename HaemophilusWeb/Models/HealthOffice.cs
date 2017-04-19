using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FluentValidation.Attributes;
using HaemophilusWeb.Validators;

namespace HaemophilusWeb.Models
{
    [Validator(typeof(HealthOfficeValidator))]
    public class HealthOffice
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int HealthOfficeId { get; set; }

        [Display(Name = "Adresse")]
        public string Address { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        [Display(Name = "Telefon")]
        [Phone]
        public string Phone { get; set; }

        [Phone]
        public string Fax { get; set; }

        [Display(Name = "Postleitzahl")]
        public string PostalCode { get; set; }
    }
}