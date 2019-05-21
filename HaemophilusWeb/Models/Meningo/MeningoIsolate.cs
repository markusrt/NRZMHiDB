using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Script.Serialization;

namespace HaemophilusWeb.Models.Meningo
{
    public class MeningoIsolate : MeningoIsolateBase, ISendingReference<MeningoSending, MeningoPatient>
    {
        [Key]
        [ForeignKey("Sending")]
        [Display(Name = "Einsendung")]
        public int MeningoSendingId { get; set; }

        [ScriptIgnore]
        public MeningoSending Sending { get; set; }

        public virtual NeisseriaPubMlstIsolate PubMlstIsolate { get; set; }

        public override string ToString()
        {
            return
                $"{StemNumber}: {Agglutination} - {RibosomalRna16S} - {RibosomalRna16SBestMatch} - {RplF} - {Remark} - {PorAPcr} ({PorAVr1}/{PorAVr2})";
        }
    }
}