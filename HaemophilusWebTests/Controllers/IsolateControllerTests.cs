using FluentAssertions;
using HaemophilusWeb.Models;
using HaemophilusWeb.ViewModels;
using NUnit.Framework;

namespace HaemophilusWeb.Controllers
{
    public class IsolateControllerTests
    {
        [Test]
        public void ParseAndMapLaboratoryNumber_ValidLaboratoryNumber_AssignsYearAndSequentialNumberCorrectly()
        {
            var isolateViewModel = new IsolateViewModel();
            isolateViewModel.LaboratoryNumber = "0000123/15";
            var isolate = new Isolate();

            IsolateController.ParseAndMapLaboratoryNumber(isolateViewModel, isolate);

            isolate.Year.Should().Be(2015);
            isolate.YearlySequentialIsolateNumber.Should().Be(123);
        }
    }
}