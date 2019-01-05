using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Script.Serialization;

namespace HaemophilusWeb.Models.Meningo
{
    public class MeningoIsolate : MeningoIsolateBase
    {
        [Key]
        [ForeignKey("Sending")]
        [Display(Name = "Einsendung")]
        public int MeningoSendingId { get; set; }

        [ScriptIgnore]
        public MeningoSending Sending { get; set; }
    }
}