using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Mvc;
using FluentAssertions;
using HaemophilusWeb.Models;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using Moq;
using NSubstitute;
using NSubstitute.ReceivedExtensions;
using NUnit.Framework;

namespace HaemophilusWeb.Controllers
{
    public class AccountControllerTests
    {
        private IUserStore<ApplicationUser> _userStore;
        private UserManager<ApplicationUser> _userManager;
        private IAuthenticationManager _authenticationManager;

        [SetUp]
        public void Setup()
        {
            _userStore = Substitute.For<IUserStore<ApplicationUser>>();
            _userManager = Substitute.For<UserManager<ApplicationUser>>(_userStore);
            _authenticationManager = Substitute.For<IAuthenticationManager>();
        }

        [Test]
        public void Ctor_DoesNotThrow()
        {
            var sut = CreateAccountController();

            sut.Should().NotBeNull();
        }


        [Test]
        public void Login_AddsReturnUrlToViewBag()
        {
            var sut = CreateAccountController();

            var actionResult = sut.Login("/foo/bar");

            var viewResult = actionResult.Should().BeOfType<ViewResult>().And.Subject.As<ViewResult>();
            string returnUrl = viewResult.ViewBag.ReturnUrl;
            returnUrl.Should().Be("/foo/bar");
        }

        [Test]
        public async Task LoginUserNotFound_AddsModelError()
        {
            var sut = CreateAccountController();
            _userManager.FindAsync("user", "password").Returns(Task.FromResult((ApplicationUser)null));

            await sut.Login(new LoginViewModel() {UserName = "user", Password = "password"}, "/foo/bar");

            sut.ModelState[""].As<ModelState>().Errors.ToList()
                .Should().Contain(e => e.ErrorMessage.Equals("Ungültiger Benutzername oder Passwort."));
        }

        [Test]
        public async Task LoginUserFound_SignsInAndRedirectsToLocalPage()
        {
            var appUser = new ApplicationUser();
            var identity = new ClaimsIdentity();
            var sut = CreateAccountController();
            
            sut.Url.IsLocalUrl(Arg.Any<string>()).Returns(true);
            _userManager.FindAsync("user", "password").Returns(Task.FromResult(appUser));
            _userManager.CreateIdentityAsync(appUser, DefaultAuthenticationTypes.ApplicationCookie).Returns(identity);

            var actionResult = await sut.Login(new LoginViewModel() { UserName = "user", Password = "password" }, "/foo/bar");

            var redirectResult = actionResult.Should().BeOfType<RedirectResult>().And.Subject.As<RedirectResult>();
            redirectResult.Url.Should().Be("/foo/bar");

            _authenticationManager.Received().SignIn(Arg.Any<AuthenticationProperties>(), identity);
        }

        [Test]
        public async Task LoginUserFound_SignsInAndRedirectsToIndexForRemoteUrls()
        {
            var appUser = new ApplicationUser();
            var sut = CreateAccountController();

            sut.Url.IsLocalUrl(Arg.Any<string>()).Returns(false);
            _userManager.FindAsync("user", "password").Returns(Task.FromResult(appUser));

            var actionResult =
                await sut.Login(new LoginViewModel() {UserName = "user", Password = "password"}, "http://www.google.de");

            var redirectResult = actionResult.Should().BeOfType<RedirectToRouteResult>().And.Subject
                .As<RedirectToRouteResult>();
            redirectResult.RouteValues["action"].Should().Be("Index");
            redirectResult.RouteValues["controller"].Should().Be("Home");
        }

        [Test]
        public void LogOff_CallsSignOutAndRedirectsToIndex()
        {
            var sut = CreateAccountController();

            var actionResult = sut.LogOff();

            var redirectResult = actionResult.Should().BeOfType<RedirectToRouteResult>().And.Subject
                .As<RedirectToRouteResult>();
            redirectResult.RouteValues["action"].Should().Be("Index");
            redirectResult.RouteValues["controller"].Should().Be("Home");

            _authenticationManager.Received().SignOut();
        }

        private AccountController CreateAccountController()
        {
            return new AccountController(_userManager, _authenticationManager)
            { 
                Url = Substitute.For<UrlHelper>()
            };
        }
    }
}