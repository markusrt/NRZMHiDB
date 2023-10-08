using FluentAssertions;
using NUnit.Framework;

namespace HaemophilusWeb.Models
{
    public class SamplingLocationExtensionsTests
    {
        [TestCase(SamplingLocation.OtherNonInvasive, true)]
        [TestCase(SamplingLocation.OtherInvasive, true)]
        [TestCase(SamplingLocation.Blood, false)]
        public void IsOther_DetectsOtherLocations(SamplingLocation samplingLocation, bool expectedIsOther)
        {
            samplingLocation.IsOther().Should().Be(expectedIsOther);
        }
    }
}