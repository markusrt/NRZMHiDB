using FluentAssertions;
using NUnit.Framework;

namespace HaemophilusWeb.Models.Meningo
{
    [TestFixture]
    public class MeningoSendingTests
    {
        [TestCase(MeningoSamplingLocation.AnalSwab, YesNo.No)]
        [TestCase(MeningoSamplingLocation.Blood, YesNo.Yes)]
        [TestCase(MeningoSamplingLocation.OtherInvasive, YesNo.Yes)]
        [TestCase(MeningoSamplingLocation.OtherNonInvasive, YesNo.No)]
        public void Invasive_IsCalculated_AccordingToSamplingLocation(MeningoSamplingLocation samplingLocation, YesNo expectedInvasive)
        {
            var meningoSending = new MeningoSending {SamplingLocation = samplingLocation};
            meningoSending.Invasive.Should().Be(expectedInvasive);
        }

        [TestCase(MeningoMaterial.NativeMaterial, false)]
        [TestCase(MeningoMaterial.IsolatedDna, false)]
        [TestCase(MeningoMaterial.NoGrowth, true)]
        [TestCase(MeningoMaterial.VitalStem, true)]
        public void AutoAssignStemNumber_Material_IsSetAccordingToMaterial(MeningoMaterial material,
            bool expectedAutoAssign)
        {
            var sending = new MeningoSending {Material = material};

            sending.AutoAssignStemNumber.Should().Be(expectedAutoAssign);
        }
    }
}