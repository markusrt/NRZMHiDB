using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HaemophilusWeb.Models.Meningo;

namespace HaemophilusWeb.Models
{
    public class NeisseriaPubMlstIsolate : INeisseriaIsolateAlleleProperties
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [ForeignKey("MeningoIsolate")]
        public int NeisseriaPubMlstIsolateId { get; set; }

        public string PorAVr1 { get; set; }
        public string PorAVr2 { get; set; }
        public string FetAVr { get; set; }
        public string PorB { get; set; }
        public string Fhbp { get; set; }
        public string Nhba { get; set; }
        public string NadA { get; set; }
        public string PenA { get; set; }
        public string GyrA { get; set; }
        public string ParC { get; set; }
        public string ParE { get; set; }
        public string RpoB { get; set; }
        public string RplF { get; set; }
        public string SequenceType { get; set; }
        public string ClonalComplex { get; set; }

        public virtual MeningoIsolate MeningoIsolate { get; set; }
    }
}