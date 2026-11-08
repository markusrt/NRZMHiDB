using System.Linq;
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

        // specs/019 FR-011, US2.1: the lookup asks the next database only when the previous one found nothing
        [Test]
        public void NeisseriaIsolates_FirstServiceWithoutRecord_ReturnsRecordOfSecondService()
        {
            var first = Substitute.For<NeisseriaPubMlstService>();
            var second = Substitute.For<NeisseriaPubMlstService>();
            second.GetIsolateByReference("DE1234").Returns(new NeisseriaPubMlstIsolate { PubMlstId = 42, Database = "second" });
            var controller = new PubMlstController(first, second);

            var result = controller.NeisseriaIsolates("DE1234");

            ((NeisseriaPubMlstIsolate) ((JsonResult) result).Data).PubMlstId.Should().Be(42);
            first.Received(1).GetIsolateByReference("DE1234");
        }

        // specs/019 FR-011, US2.1: the first record found wins, the next database is not asked
        [Test]
        public void NeisseriaIsolates_FirstServiceFindsRecord_DoesNotAskSecond()
        {
            var first = Substitute.For<NeisseriaPubMlstService>();
            var second = Substitute.For<NeisseriaPubMlstService>();
            first.GetIsolateByReference("DE1234").Returns(new NeisseriaPubMlstIsolate { PubMlstId = 7, Database = "first" });
            var controller = new PubMlstController(first, second);

            var result = controller.NeisseriaIsolates("DE1234");

            ((NeisseriaPubMlstIsolate) ((JsonResult) result).Data).PubMlstId.Should().Be(7);
            second.DidNotReceive().GetIsolateByReference(Arg.Any<string>());
        }

        // specs/019 FR-011: neither database returns a record, the lookup answers "not found"
        [Test]
        public void NeisseriaIsolates_NoServiceFindsRecord_ReturnsNotFound()
        {
            var controller = new PubMlstController(Substitute.For<NeisseriaPubMlstService>(),
                Substitute.For<NeisseriaPubMlstService>());

            controller.NeisseriaIsolates("DE1234").Should().BeOfType<HttpNotFoundResult>();
        }

        // specs/019 FR-002, US4.1: the lookup is a POST and requires the standard laboratory-user role
        [Test]
        public void NeisseriaIsolates_IsPostForStandardRole()
        {
            var action = typeof(PubMlstController).GetMethod(nameof(PubMlstController.NeisseriaIsolates));

            action.IsDefined(typeof(HttpPostAttribute), true).Should().BeTrue();
            action.GetCustomAttributes(typeof(AuthorizeAttribute), true).Cast<AuthorizeAttribute>().Single().Roles
                .Should().Be(DefaultRoles.User);
        }
    }
}