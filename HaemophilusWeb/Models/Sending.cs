using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Script.Serialization;
using FluentValidation.Attributes;
using FluentValidation.Validators;
using HaemophilusWeb.Validators;
using Microsoft.Ajax.Utilities;

namespace HaemophilusWeb.Models
{
    [Validator(typeof(SendingValidator))]
    public class Sending
    {
        public Sending()
        {
            ReceivingDate = DateTime.Now;
            SamplingDate = DateTime.Now.Subtract(TimeSpan.FromDays(7));
            SenderConclusion = "H. influenzae";
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "Einsendungsnummer")]
        public int SendingId { get; set; }

        [Display(Name = "Einsender")]
        public int SenderId { get; set; }

        [Display(Name = "Patient")]
        public int PatientId { get; set; }

        [Display(Name = "Entnahmedatum")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = true)]
        public DateTime SamplingDate { get; set; }

        [DisplayName("Eingangsdatum")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = true)]
        public DateTime ReceivingDate { get; set; }

        [Display(Name = "Material")]
        public Material Material { get; set; }

        [Display(Name = "Anderes Material")]
        public string OtherMaterial { get; set; }

        [Display(Name = "Invasiv")]
        public YesNo? Invasive { get; set; }

        [Display(Name = "Labnr. Einsender")]
        public string SenderLaboratoryNumber { get; set; }

        [Display(Name = "Ergebnis Einsender")]
        public string SenderConclusion { get; set; }

        [Display(Name = "Beurteilung")]
        public Evaluation Evaluation { get; set; }

        [Display(Name = "Bemerkung")]
        public string Remark { get; set; }

        [Display(Name = "Befund am")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? ReportDate { get; set; }

        public virtual Isolate Isolate { get; set; }
    }
}