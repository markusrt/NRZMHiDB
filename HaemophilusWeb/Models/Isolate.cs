using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Script.Serialization;

namespace HaemophilusWeb.Models
{
    public class Isolate : IsolateBase
    {
        [Display(Name = "Einsendung")]
        [Key]
        [ForeignKey("Sending")]
        public int SendingId { get; set; }

        [ScriptIgnore]
        public Sending Sending { get; set; }

        public int YearlySequentialIsolateNumber { get; set; }

        public int Year { get; set; }

        [Display(Name = "Labornummer")]
        public string LaboratoryNumber
        {
            get { return string.Format("{0:000}/{1}", YearlySequentialIsolateNumber, Year - 2000); }
        }

        public virtual ICollection<EpsilometerTest> EpsilometerTests { get; set; }
    }
}