using FluentAssertions;
using NUnit.Framework;

namespace HaemophilusWeb.Models
{
    public class CountyTests
    {
        [Test]
        [TestCase("Hamburg", "Hamburg, Freie und Hansestadt")]
        [TestCase("Städteregion Aachen", "Aachen")]
        [TestCase("Berlin", "Kreisfreie Stadt Berlin")]
        [TestCase("Region Hannover", "Hannover")]
        [TestCase(null, null)]
        public void IsEqualTo_NameVariations_ReturnsTrue(string officialName, string nameVariation)
        {
            var county = new County {Name = officialName};

            county.IsEqualTo(nameVariation).Should().BeTrue();
        }

        [TestCase("Hofstetten", "Hof")]
        [TestCase(null, "Kreisfreie Stadt Berlin")]
        [TestCase("Berlin", null)]
        public void IsEqualTo_NameVariations_ReturnsFalse(string officialName, string nameVariation)
        {
            var county = new County {Name = officialName};

            county.IsEqualTo(nameVariation).Should().BeFalse();
        }

        // specs/025 FR-013, US1.1, US1.2: comma suffix, fill words and case are ignored on both sides
        [TestCase("Flensburg, Stadt", "flensburg")]
        [TestCase("Flensburg, Stadt", "Landkreis Flensburg")]
        [TestCase("Flensburg, Stadt", "Kreisfreie Stadt Flensburg")]
        [TestCase("Landkreis Forchheim", "Forchheim")]
        [TestCase("Kreis Lippe", "LIPPE")]
        public void IsEqualTo_CommaSuffixFillWordsAndCase_ReturnsTrue(string officialName, string nameVariation)
        {
            var county = new County {Name = officialName};

            county.IsEqualTo(nameVariation).Should().BeTrue();
        }

        // specs/025 FR-013, edge case "Normalization is symmetric and literal": only case is folded, fill words are
        // removed in the listed spelling only, and a comma in first position is kept
        [TestCase("Lübeck, Hansestadt", "Lubeck")]
        [TestCase("Forchheim", "landkreis Forchheim")]
        [TestCase("Hof", ",Hof")]
        [TestCase("Bad Tölz-Wolfratshausen", "Bad Tölz Wolfratshausen")]
        public void IsEqualTo_OnlyCaseIsFolded_ReturnsFalse(string officialName, string nameVariation)
        {
            var county = new County {Name = officialName};

            county.IsEqualTo(nameVariation).Should().BeFalse();
        }
    }
}