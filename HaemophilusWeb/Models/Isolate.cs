using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Script.Serialization;
using HaemophilusWeb.Utils;

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
            get { return ReportFormatter.ToLaboratoryNumber(YearlySequentialIsolateNumber, Year); }
        }


        public virtual ICollection<EpsilometerTest> EpsilometerTests { get; set; }
    }
}