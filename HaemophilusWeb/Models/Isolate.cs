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

        public virtual ICollection<EpsilometerTest> EpsilometerTests { get; set; }
    }
}