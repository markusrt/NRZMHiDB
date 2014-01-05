using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;

namespace HaemophilusWeb.Models
{
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
        public int SendingId { get; set; }

        [Display(Name = "Einsender")]
        public int SenderId { get; set; }

        [Display(Name = "Patient")]
        public int PatientId { get; set; }
        
        [Display(Name = "Entnahmedatum")]
        public DateTime SamplingDate { get; set; }

        [Display(Name = "Eingangsdatum")]
        public DateTime ReceivingDate { get; set; }
        
        [Display(Name = "Material")]
        public Material Material { get; set; }

        [Display(Name = "anderes Material")]
        public string OtherMaterial { get; set; }

        [Display(Name = "Invasiv")]
        public bool? Invasive { get; set; }

        [Display(Name = "Labnr. Einsender")]
        public string SenderLaboratoryNumber { get; set; }

        [Display(Name = "Ergebnis Einsender")]
        [Required]
        public string SenderConclusion { get; set; }

        [Display(Name = "Beurteilung")]
        public string Evaluation { get; set; }
    }

    public enum Material
    {
        [Description("Blut")]
        Blood,
        [Description("Liquor")]
        Liquor
    }

}