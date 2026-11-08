using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using FluentAssertions;
using HaemophilusWeb.Mocks;
using HaemophilusWeb.Models;
using HaemophilusWeb.Models.Meningo;
using NSubstitute;
using NUnit.Framework;

namespace HaemophilusWeb.Controllers
{
    public class MeningoIsolateControllerTests
    {
        private MeningoIsolateController controller;

        [SetUp]
        public void SetUp()
        {
            // Own empty set instead of the shared static store of ApplicationDbContextMock
            var db = Substitute.For<IApplicationDbContext>();
            db.MeningoIsolates.Returns(new InMemoryDbSet<MeningoIsolate>(new HashSet<MeningoIsolate>()));
            controller = new MeningoIsolateController(db);
        }

        // specs/005 FR-024: the edit form without an id is a bad request
        [Test]
        public void Edit_WithoutId_ReturnsBadRequest()
        {
            var result = controller.Edit((int?) null);

            ((HttpStatusCodeResult) result).StatusCode.Should().Be((int) HttpStatusCode.BadRequest);
        }

        // specs/005 FR-024: an unknown isolate is not found
        [Test]
        public void Edit_UnknownId_ReturnsNotFound()
        {
            var result = controller.Edit(int.MaxValue);

            result.Should().BeOfType<HttpNotFoundResult>();
        }

        // specs/005 FR-023: every action requires the standard laboratory-user role
        [Test]
        public void Controller_RequiresStandardRole()
        {
            typeof(MeningoIsolateController).GetCustomAttributes(typeof(AuthorizeAttribute), true)
                .Cast<AuthorizeAttribute>().Single().Roles.Should().Be(DefaultRoles.User);
        }

        // specs/005 FR-023: the save requires the anti-forgery token
        [Test]
        public void EditPost_ValidatesAntiForgeryToken()
        {
            typeof(MeningoIsolateController).GetMethods()
                .Single(m => m.Name == "Edit" && m.IsDefined(typeof(HttpPostAttribute), true))
                .IsDefined(typeof(ValidateAntiForgeryTokenAttribute), true).Should().BeTrue();
        }

        // specs/019 FR-003, US4.3: the matching submission requires the anti-forgery token
        [Test]
        public void PubMlstMatchPost_ValidatesAntiForgeryToken()
        {
            typeof(MeningoIsolateController).GetMethods()
                .Single(m => m.Name == "PubMlstMatch" && m.IsDefined(typeof(HttpPostAttribute), true))
                .IsDefined(typeof(ValidateAntiForgeryTokenAttribute), true).Should().BeTrue();
        }
    }
}
