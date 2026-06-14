using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HaemophilusWeb.Models
{
    public class ConfigurationEntry
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ConfigurationEntryId { get; set; }

        [Required]
        [StringLength(400)]
        [Index(IsUnique = true)]
        public string Key { get; set; }

        public string Value { get; set; }
    }
}
