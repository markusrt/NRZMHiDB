using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Web.Script.Serialization;
using FluentValidation;
using FluentValidation.Attributes;

namespace HaemophilusWeb.Models
{
    public class Isolate
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "Stammnummer")]
        public int IsolateId { get; set; }

        [Display(Name = "Einsendung")]
        [Key, ForeignKey("Sending")]
        public int SendingId { get; set; }

        [Required]
        [ScriptIgnore]
        public Sending Sending { get; set; }

        public int YearlySequentialIsolateNumber { get; set; }

        public int Year { get; set; }

        [Display(Name = "Labornummer")]
        public string LaboratoryNumber
        {
            get
            {
                return string.Format("{0}/{1}", YearlySequentialIsolateNumber, Year - 2000);
            }
        }

    }
}