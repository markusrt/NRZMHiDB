using System.ComponentModel.DataAnnotations;

namespace HaemophilusWeb.Models
{
    public class PubMlstMatchInfo
    {
        [Display(Name = "PubMLST ID")]
        public int IsolateId { get; set; }
        
        public int? NeisseriaPubMlstIsolateId { get; set; }

        [Display(Name = "PubMLST ID")]
        public int? PubMlstId { get; set; }

        [Display(Name = "Datenbank")]
        public string Database { get; set; }

        [Display(Name = "Stammnummer")]
        public string StemNumber { get; set; }

        [Display(Name = "Labornummer")]
        public string LaboratoryNumber { get; set; }
    }
}