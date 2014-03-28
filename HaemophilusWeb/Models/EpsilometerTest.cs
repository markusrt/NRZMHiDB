using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HaemophilusWeb.Models
{
    public class EpsilometerTest
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int EpsilometerTestId { get; set; }

        public int EucastClinicalBreakpointId { get; set; }
        public virtual EucastClinicalBreakpoint EucastClinicalBreakpoint { get; set; }

        public float Measurement { get; set; }

        public EpsilometerTestResult Result { get; set; }
    }
}