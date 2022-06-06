using System.Web.Mvc;
using System.Web.Script.Serialization;
using FluentAssertions;
using HaemophilusWeb.Models;
using HaemophilusWeb.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NSubstitute;
using NUnit.Framework;

namespace HaemophilusWeb.Controllers
{
    [TestFixture]
    public class PubMlstControllerTests
    {
        [Test]
        public void NeisseriaIsolates_NonExistingIsolate_ReturnsNotFound()
        {
            var service = Substitute.For<NeisseriaPubMlstService>();
            var controller = new PubMlstController(service);

            var isolate = controller.NeisseriaIsolates("ABC");

            isolate.Should().BeOfType<HttpNotFoundResult>();
        }

        [Test]
        public void NeisseriaIsolates_ExistingIsolate_FieldsAreSet()
        {
            const string isolateReference = "DE1234";
            var service = Substitute.For<NeisseriaPubMlstService>();
            service.GetIsolateByReference(isolateReference).Returns(new NeisseriaPubMlstIsolate { SequenceType = "22" });

            var controller = new PubMlstController(service);

            var json = new JavaScriptSerializer().Serialize((controller.NeisseriaIsolates(isolateReference) as JsonResult)?.Data);
            var isolate = JsonConvert.DeserializeObject<NeisseriaPubMlstIsolate>(json);
            isolate.SequenceType.Should().Be("22");
        }
    }
}