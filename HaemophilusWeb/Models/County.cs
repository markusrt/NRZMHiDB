using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace HaemophilusWeb.Models
{
    public class County
    {
        private static readonly IEnumerable<string> FillWords = new List<string>
        {
            "Städteregion",
            "Kreisfreie Stadt",
            "Region",
        };

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CountyId { get; set; }

        public string CountyNumber { get; set; }

        public string Name { get; set; }

        public DateTime ValidSince { get; set; }

        public bool IsEqualTo(string nameOfAnotherCountry)
        {
            var name = TrimNonNameParts(Name);
            var otherName = TrimNonNameParts(nameOfAnotherCountry);
            return string.Equals(name, otherName, StringComparison.OrdinalIgnoreCase);
        }

        private static string TrimNonNameParts(string value)
        {
            var trimmedValue = value;
            if (value != null)
            {
                trimmedValue = TrimSuffix(value);
                trimmedValue = RemoveWellKnownFillWords(trimmedValue);
            }
            return trimmedValue;
        }

        private static string RemoveWellKnownFillWords(string value)
        {
            return FillWords.Aggregate(value, (current, fillWord) => current.Replace(fillWord, string.Empty).Trim());
        }

        private static string TrimSuffix(string value)
        {
            var index = value.IndexOf(',');
            return index > 0 ? value.Substring(0, index) : value;
        }
    }
}