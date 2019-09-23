using FluentAssertions;
using HaemophilusWeb.Models;
using HaemophilusWeb.Models.Meningo;
using HaemophilusWeb.ViewModels;
using NUnit.Framework;

namespace HaemophilusWeb.Automapper
{
    public class MeningoIsolateViewModelMappingActionTests
    {
        [TestCase(MeningoMaterial.IsolatedDna, MeningoMaterial.NoGrowth)]
        [TestCase(MeningoMaterial.VitalStem, MeningoMaterial.NoGrowth)]
        [TestCase(MeningoMaterial.NoGrowth, MeningoMaterial.NoGrowth)]
        [TestCase(MeningoMaterial.NativeMaterial, MeningoMaterial.NativeMaterial)]
        public void Process_NoGrowthAtAll_SetFieldMeningoMaterialToNoGrowthExceptForNativeMaterial(MeningoMaterial currentMaterial, MeningoMaterial expectedMaterial)
        {
            var sut = new MeningoIsolateViewModelMappingAction();
            var isolateViewModel = new MeningoIsolateViewModel();
            var isolate = new MeningoIsolate {Sending = new MeningoSending() {Material = currentMaterial}};

            sut.Process(isolateViewModel, isolate);

            isolate.Sending.Material.Should().Be(expectedMaterial);
        }
    }
}