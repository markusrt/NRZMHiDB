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
    }
}