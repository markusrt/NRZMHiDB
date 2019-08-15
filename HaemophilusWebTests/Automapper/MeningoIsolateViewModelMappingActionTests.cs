using FluentAssertions;
using HaemophilusWeb.Models;
using HaemophilusWeb.Models.Meningo;
using HaemophilusWeb.ViewModels;
using NUnit.Framework;

namespace HaemophilusWeb.Automapper
{
    public class MeningoIsolateViewModelMappingActionTests
    {
        [Test]
        public void Process_NoGrowthAtAll_SetFieldMeningoMaterialToNoGrowth()
        {
            var sut = new MeningoIsolateViewModelMappingAction();
            var isolateViewModel = new MeningoIsolateViewModel();
            var isolate = new MeningoIsolate();
            isolate.Sending = new MeningoSending();

            sut.Process(isolateViewModel, isolate);

            isolate.Sending.Material.Should().Be(MeningoMaterial.NoGrowth);
        }
    }
}