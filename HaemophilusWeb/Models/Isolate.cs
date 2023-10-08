using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Script.Serialization;
using HaemophilusWeb.Utils;

namespace HaemophilusWeb.Models
{
    public class Isolate : IsolateBase, ISendingReference<Sending, Patient>
    {
        [Display(Name = "Einsendung")]
        [Key]
        [ForeignKey("Sending")]
        public int SendingId { get; set; }

        [ScriptIgnore]
        public Sending Sending { get; set; }

        [Display(Name = "api NH")]
        public UnspecificTestResult ApiNh { get; set; }

        [Display(Name = "api NH beste Übereinstimmung")]
        public string ApiNhBestMatch { get; set; }

        [Display(Name = "api NH Übereinstimmung")]
        public double? ApiNhMatchInPercent { get; set; }
    }
}