using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using FluentAssertions;
using HaemophilusWeb.Models;
using NUnit.Framework;

namespace HaemophilusWeb.Controllers
{
    /// <summary>
    /// specs/021 access matrix (FR-001, FR-004, FR-006, FR-010). Asserts only the cells no open backlog item
    /// disputes: whether the Administrator role is admitted to laboratory functions (DEC-001), the LGA export for
    /// Public Health (DEC-006), and the functions that check no role (report pages, health offices, RKI match
    /// import, country list; SEC-004, SEC-011) are deliberately left out.
    /// </summary>
    public class AuthorizationMatrixTests
    {
        private const string Anonymous = "anonymous";
        private const string SignedIn = "signed-in";

        private static readonly string[] LaboratoryControllers =
        {
            "SenderController", "MeningoSenderController", "PatientSendingController", "MeningoPatientSendingController",
            "IsolateController", "MeningoIsolateController", "PatientController", "MeningoPatientController",
            "SendingController", "MeningoSendingController", "ConfigurationController", "CountyController",
            "EucastClinicalBreakpointsController", "PubMlstController"
        };

        // specs/021 FR-001: every page requires a sign-in unless it opts out
        [Test]
        public void GlobalFilters_RequireSignInForEveryAction()
        {
            var filters = new GlobalFilterCollection();

            FilterConfig.RegisterGlobalFilters(filters);

            filters.Select(f => f.Instance).OfType<AuthorizeAttribute>()
                .Should().ContainSingle(a => string.IsNullOrEmpty(a.Roles) && string.IsNullOrEmpty(a.Users));
        }

        // specs/021 FR-001: only sign-in, registration, the external-login pages and the EpiscanGIS export are open
        [Test]
        public void OnlySignInRegistrationExternalLoginAndEpiscanGisExportAdmitAnonymousRequests()
        {
            var anonymous = Actions().Where(a => Access(a.Controller, a.Action) == Anonymous)
                .Select(a => a.Controller.Name + "." + a.Action.Name).Distinct();

            anonymous.Should().BeEquivalentTo(
                "AccountController.Login", "AccountController.Register", "AccountController.ExternalLogin",
                "AccountController.ExternalLoginCallback", "AccountController.ExternalLoginConfirmation",
                "AccountController.ExternalLoginFailure", "EpiscanGisExportController.Index");
        }

        // specs/021 FR-004, US5.5: every access check names one of the three fixed roles, so further roles grant nothing
        [Test]
        public void EveryRoleCheckNamesOnlyTheThreeFixedRoles()
        {
            var known = DefaultRoles.GetAll().ToList();
            var named = Actions()
                .SelectMany(a => AuthorizeAttributes(a.Controller, a.Action))
                .SelectMany(a => SplitRoles(a.Roles))
                .Distinct();

            named.Should().OnlyContain(r => known.Contains(r));
            known.Should().BeEquivalentTo("Standardbenutzer", "Administrator", "RKI");
        }

        // specs/021 FR-010, US3.2, US5.3: the role pages admit only the Administrator role
        [Test]
        public void RolePagesAdmitOnlyTheAdministratorRole()
        {
            var access = Actions().Where(a => a.Controller == typeof(RoleController))
                .Select(a => Access(a.Controller, a.Action)).Distinct();

            access.Should().BeEquivalentTo(DefaultRoles.Administrator);
        }

        // specs/021 FR-003, FR-011, US1.7, US8.1: sign-out, own password page and home/"Über" need a sign-in, no role
        [TestCase("AccountController", "Manage")]
        [TestCase("AccountController", "LogOff")]
        [TestCase("HomeController", "Index")]
        [TestCase("HomeController", "About")]
        [TestCase("HomeController", "Change")]
        public void AccountAndHomePagesNeedOnlyASignIn(string controller, string action)
        {
            Actions().Where(a => a.Controller.Name == controller && a.Action.Name == action)
                .Select(a => Access(a.Controller, a.Action)).Distinct()
                .Should().BeEquivalentTo(SignedIn);
        }

        // specs/021 US3.1, US3.3, US4.3: every laboratory function admits the Standard User role, and Public Health
        // only for the two RKI exports (the LGA export, DEC-006, and the Administrator cells, DEC-001, stay unasserted)
        [Test]
        public void LaboratoryFunctionsAdmitStandardUserAndPublicHealthOnlyForTheRkiExports()
        {
            var laboratory = Actions().Where(a => LaboratoryControllers.Contains(a.Controller.Name)).ToList();
            laboratory.Select(a => a.Controller.Name).Distinct().Should().BeEquivalentTo(LaboratoryControllers);

            foreach (var (controller, action) in laboratory)
            {
                var roles = SplitRoles(Access(controller, action)).ToList();
                var id = controller.Name + "." + action.Name;
                roles.Should().Contain(DefaultRoles.User, id);
                if (action.Name == "RkiExport")
                {
                    roles.Should().Contain(DefaultRoles.PublicHealth, id);
                }
                else if (id != "MeningoPatientSendingController.StateAuthorityExport")
                {
                    roles.Should().NotContain(DefaultRoles.PublicHealth, id);
                }
            }
        }

        // specs/021 US4.3: no function outside the RKI and LGA exports names the Public Health role
        [Test]
        public void PublicHealthIsNamedOnlyByTheRkiAndLgaExports()
        {
            var named = Actions()
                .Where(a => AuthorizeAttributes(a.Controller, a.Action).Any(x => SplitRoles(x.Roles).Contains(DefaultRoles.PublicHealth)))
                .Select(a => a.Controller.Name + "." + a.Action.Name).Distinct();

            named.Should().BeSubsetOf(new[]
            {
                "PatientSendingController.RkiExport", "MeningoPatientSendingController.RkiExport",
                "MeningoPatientSendingController.StateAuthorityExport"
            });
        }

        private static IEnumerable<(Type Controller, MethodInfo Action)> Actions()
        {
            var assembly = typeof(HomeController).Assembly;
            return assembly.GetTypes()
                .Where(t => typeof(Controller).IsAssignableFrom(t) && !t.IsAbstract && t.IsPublic)
                .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                    .Where(m => !m.IsSpecialName && m.DeclaringType.Assembly == assembly
                                && !m.IsDefined(typeof(NonActionAttribute), true))
                    .Select(m => (t, m)));
        }

        private static IEnumerable<AuthorizeAttribute> AuthorizeAttributes(Type controller, MethodInfo action)
        {
            return action.GetCustomAttributes(typeof(AuthorizeAttribute), true).Cast<AuthorizeAttribute>()
                .Concat(controller.GetCustomAttributes(typeof(AuthorizeAttribute), true).Cast<AuthorizeAttribute>());
        }

        /// <summary>
        /// Effective access as MVC evaluates it: [AllowAnonymous] on action or controller skips every authorization
        /// filter; otherwise all [Authorize] filters (action, controller, global) must pass, so the admitted roles are
        /// the intersection of the non-empty role lists, and "signed-in" when none names roles.
        /// </summary>
        private static string Access(Type controller, MethodInfo action)
        {
            if (action.IsDefined(typeof(AllowAnonymousAttribute), true) ||
                controller.IsDefined(typeof(AllowAnonymousAttribute), true))
            {
                return Anonymous;
            }

            var roleSets = AuthorizeAttributes(controller, action)
                .Select(a => SplitRoles(a.Roles).ToList())
                .Where(r => r.Any())
                .ToList();
            if (!roleSets.Any())
            {
                return SignedIn;
            }

            return string.Join(",", roleSets.Aggregate((a, b) => a.Intersect(b).ToList()).OrderBy(r => r));
        }

        private static IEnumerable<string> SplitRoles(string roles)
        {
            return (roles ?? string.Empty).Split(',').Select(r => r.Trim()).Where(r => r.Length > 0);
        }
    }
}
