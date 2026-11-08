using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using FluentAssertions;
using HaemophilusWeb.Mocks;
using HaemophilusWeb.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security;
using NSubstitute;
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

        // specs/021 US6.1, FR-007: without the configured token the registration address shows the sign-in view
        [TestCase(null)]
        [TestCase("")]
        [TestCase("SV-021-wrong")]
        public void Register_WithoutConfiguredToken_ShowsLoginView(string token)
        {
            using (new RegistrationTokenScope(TestRegistrationToken))
            {
                var sut = CreateAccountController();

                var viewResult = sut.Register(token).Should().BeOfType<ViewResult>().Subject;

                viewResult.ViewName.Should().Be("Login");
                viewResult.Model.Should().BeNull();
            }
        }

        // specs/021 US6.2, FR-007: the configured token opens the form with a choice of all stored roles
        [Test]
        public void Register_WithConfiguredToken_ShowsFormWithAllStoredRoles()
        {
            using (new RegistrationTokenScope(TestRegistrationToken))
            {
                var sut = CreateAccountController();
                ReplaceStoredRoles(sut, DefaultRoles.User, DefaultRoles.Administrator, DefaultRoles.PublicHealth, "SV-021-Rolle");

                var viewResult = sut.Register(TestRegistrationToken).Should().BeOfType<ViewResult>().Subject;

                viewResult.ViewName.Should().BeEmpty();
                viewResult.Model.Should().BeOfType<RegisterViewModel>();
                SelectList roles = viewResult.ViewBag.Roles;
                roles.Select(r => r.Value).Should().BeEquivalentTo(
                    DefaultRoles.User, DefaultRoles.Administrator, DefaultRoles.PublicHealth, "SV-021-Rolle");
            }
        }

        // specs/021 US6.4, FR-007, SC-004: a successful registration assigns exactly the chosen role, signs the browser
        // in as the new account (not persistent) and shows the home page
        [Test]
        public async Task Register_Valid_CreatesAccountAssignsChosenRoleSignsInAsNewAccountAndRedirectsHome()
        {
            ApplicationUser createdUser = null;
            var identity = new ClaimsIdentity();
            _userManager.CreateAsync(Arg.Do<ApplicationUser>(u => createdUser = u), "SV021secret")
                .Returns(Task.FromResult(IdentityResult.Success));
            _userManager.AddToRoleAsync(Arg.Any<string>(), Arg.Any<string>())
                .Returns(Task.FromResult(IdentityResult.Success));
            _userManager.CreateIdentityAsync(Arg.Any<ApplicationUser>(), DefaultAuthenticationTypes.ApplicationCookie)
                .Returns(Task.FromResult(identity));
            var sut = CreateAccountController();

            var actionResult = await sut.Register(new RegisterViewModel
            {
                UserName = "SV021user", Password = "SV021secret", ConfirmPassword = "SV021secret",
                RoleName = DefaultRoles.PublicHealth
            });

            createdUser.UserName.Should().Be("SV021user");
            await _userManager.Received(1).AddToRoleAsync(Arg.Any<string>(), Arg.Any<string>());
            await _userManager.Received(1).AddToRoleAsync(createdUser.Id, DefaultRoles.PublicHealth);
            _authenticationManager.Received(1).SignIn(
                Arg.Is<AuthenticationProperties>(p => !p.IsPersistent), identity);
            var redirect = actionResult.Should().BeOfType<RedirectToRouteResult>().Subject;
            redirect.RouteValues["action"].Should().Be("Index");
            redirect.RouteValues["controller"].Should().Be("Home");
        }

        // specs/021 US6.3, FR-008: a rejected account (e.g. user name taken) is shown again with the errors, no role
        // is assigned and nobody is signed in
        [Test]
        public async Task Register_CreateFails_ShowsFormAgainWithoutRoleAssignmentOrSignIn()
        {
            _userManager.CreateAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>())
                .Returns(Task.FromResult(IdentityResult.Failed("SV-021 name taken")));
            var sut = CreateAccountController();
            ReplaceStoredRoles(sut, DefaultRoles.User);

            var actionResult = await sut.Register(new RegisterViewModel
            {
                UserName = "SV021user", Password = "SV021secret", ConfirmPassword = "SV021secret",
                RoleName = DefaultRoles.User
            });

            actionResult.Should().BeOfType<ViewResult>().Which.ViewName.Should().BeEmpty();
            sut.ModelState[""].Errors.Select(e => e.ErrorMessage).Should().Contain("SV-021 name taken");
            await _userManager.DidNotReceive().AddToRoleAsync(Arg.Any<string>(), Arg.Any<string>());
            _authenticationManager.DidNotReceive().SignIn(Arg.Any<AuthenticationProperties>(), Arg.Any<ClaimsIdentity[]>());
        }

        // specs/021 FR-008, US6.3: the account rules are the framework defaults of the application's user manager:
        // at least 6 characters, no complexity rule, unique user names of letters, digits, "@", "_" and "."
        [TestCase("SV021user", "abcdef", true)]
        [TestCase("SV021user", "abcde", false)]
        [TestCase("sv.021@x_y", "123456", true)]
        [TestCase("SV 021", "123456", false)]
        [TestCase("SV-021", "123456", false)]
        [TestCase("SV021taken", "123456", false)]
        public async Task UserManagerDefaults_DecideWhichAccountsCanBeCreated(string userName, string password,
            bool succeeds)
        {
            var store = Substitute.For<IUserStore<ApplicationUser>, IUserPasswordStore<ApplicationUser>>();
            store.FindByNameAsync(Arg.Any<string>()).Returns(Task.FromResult<ApplicationUser>(null));
            store.FindByNameAsync("SV021taken").Returns(Task.FromResult(new ApplicationUser { UserName = "SV021taken" }));
            var userManager = new UserManager<ApplicationUser>(store);

            var result = await userManager.CreateAsync(new ApplicationUser { UserName = userName }, password);

            result.Succeeded.Should().Be(succeeds);
            if (succeeds)
            {
                await store.Received(1).CreateAsync(Arg.Is<ApplicationUser>(u => u.UserName == userName));
            }
            else
            {
                await store.DidNotReceive().CreateAsync(Arg.Any<ApplicationUser>());
            }
        }

        // specs/021 US8.2, FR-011: a correct current password changes the password and leads to the success message
        [Test]
        public async Task Manage_WithCorrectCurrentPassword_ChangesPasswordAndRedirectsWithSuccessMessage()
        {
            var sut = CreateSignedInAccountController("user-021");
            _userManager.ChangePasswordAsync("user-021", "old-021", "new-021")
                .Returns(Task.FromResult(IdentityResult.Success));

            var actionResult = await sut.Manage(new ManageUserViewModel
                { OldPassword = "old-021", NewPassword = "new-021", ConfirmPassword = "new-021" });

            await _userManager.Received(1).ChangePasswordAsync("user-021", "old-021", "new-021");
            var redirect = actionResult.Should().BeOfType<RedirectToRouteResult>().Subject;
            redirect.RouteValues["action"].Should().Be("Manage");
            redirect.RouteValues["Message"].Should().Be(AccountController.ManageMessageId.ChangePasswordSuccess);

            var page = sut.Manage(AccountController.ManageMessageId.ChangePasswordSuccess).Should().BeOfType<ViewResult>().Subject;
            string statusMessage = page.ViewBag.StatusMessage;
            statusMessage.Should().Be("Ihr Passwort wurde geändert.");
        }

        // specs/021 US8.2: a rejected change (e.g. wrong current password) shows the page again with the framework error
        [Test]
        public async Task Manage_WithWrongCurrentPassword_ShowsPageAgainWithError()
        {
            var sut = CreateSignedInAccountController("user-021");
            _userManager.ChangePasswordAsync("user-021", "wrong-021", "new-021")
                .Returns(Task.FromResult(IdentityResult.Failed("Incorrect password.")));

            var actionResult = await sut.Manage(new ManageUserViewModel
                { OldPassword = "wrong-021", NewPassword = "new-021", ConfirmPassword = "new-021" });

            actionResult.Should().BeOfType<ViewResult>();
            sut.ModelState[""].Errors.Select(e => e.ErrorMessage).Should().Contain("Incorrect password.");
        }

        private const string TestRegistrationToken = "SV-021-Token";

        private static void ReplaceStoredRoles(AccountController sut, params string[] roleNames)
        {
            var context = (ApplicationDbContext)typeof(AccountController)
                .GetField("context", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(sut);
            context.Roles = new InMemoryDbSet<IdentityRole>(new HashSet<IdentityRole>(roleNames.Select(n => new IdentityRole(n))));
        }

        private AccountController CreateSignedInAccountController(string userId)
        {
            var sut = CreateAccountController();
            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId), new Claim(ClaimTypes.Name, "SV021user")
            }, "test");
            var httpContext = Substitute.For<HttpContextBase>();
            httpContext.User.Returns(new ClaimsPrincipal(identity));
            sut.ControllerContext = new ControllerContext(httpContext, new RouteData(), sut);
            _userManager.FindByIdAsync(userId)
                .Returns(Task.FromResult(new ApplicationUser { UserName = "SV021user", PasswordHash = "hash" }));
            return sut;
        }

        private sealed class RegistrationTokenScope : System.IDisposable
        {
            private const string Key = "RegistrationToken";
            private readonly string previous;

            public RegistrationTokenScope(string value)
            {
                previous = ConfigurationManager.AppSettings[Key];
                ConfigurationManager.AppSettings[Key] = value;
            }

            // The settings collection refuses Remove at run time; setting the previous value (null when absent) restores
            // what later tests read
            public void Dispose()
            {
                ConfigurationManager.AppSettings[Key] = previous;
            }
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