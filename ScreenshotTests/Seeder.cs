using System;
using System.Data.Entity;
using System.Linq;
using HaemophilusWeb.Models;
using HaemophilusWeb.Models.Meningo;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace ScreenshotTests
{
    /// <summary>
    /// Migrates the isolated test catalog and seeds deterministic, well-known data:
    /// the login user plus one record per entity at id = 1 so every id-based route renders.
    /// Runs in-process against HaemophilusWeb's own DbContext, so it always matches the current model.
    /// Idempotent.
    /// </summary>
    internal static class Seeder
    {
        public const string UserName = "Labor1";
        public const string Password = "123456";

        // Fixed dates keep screenshots stable across runs/days (avoids DateTime.Now noise in diffs).
        private static readonly DateTime Sampling = new DateTime(2020, 1, 1);
        private static readonly DateTime Receiving = new DateTime(2020, 1, 3);

        public static void Seed()
        {
            Database.SetInitializer(
                new MigrateDatabaseToLatestVersion<ApplicationDbContext, HaemophilusWeb.Migrations.Configuration>());

            using (var context = new ApplicationDbContext())
            {
                context.Database.Initialize(force: true);
            }

            SeedRolesAndUser();
            SeedDomain();
        }

        private static void SeedRolesAndUser()
        {
            using (var context = new ApplicationDbContext())
            using (var roleStore = new RoleStore<IdentityRole>(context))
            using (var roleManager = new RoleManager<IdentityRole>(roleStore))
            using (var userStore = new UserStore<ApplicationUser>(context))
            using (var userManager = new UserManager<ApplicationUser>(userStore))
            {
                var roles = new[] { DefaultRoles.Administrator, DefaultRoles.User, DefaultRoles.PublicHealth };
                foreach (var role in roles)
                {
                    if (!roleManager.RoleExists(role))
                    {
                        roleManager.Create(new IdentityRole(role));
                    }
                }

                // Relax validation: the app authenticates by password hash, so any created user can log in.
                userManager.PasswordValidator = new PasswordValidator { RequiredLength = 1 };
                userManager.UserValidator = new UserValidator<ApplicationUser>(userManager)
                {
                    AllowOnlyAlphanumericUserNames = false,
                    RequireUniqueEmail = false
                };

                if (userManager.FindByName(UserName) == null)
                {
                    var user = new ApplicationUser { UserName = UserName };
                    var result = userManager.Create(user, Password);
                    if (!result.Succeeded)
                    {
                        throw new InvalidOperationException(
                            "Failed to create test user: " + string.Join("; ", result.Errors));
                    }

                    // Grant every role so all navigation entries and admin-only views are reachable.
                    foreach (var role in roles)
                    {
                        userManager.AddToRole(user.Id, role);
                    }
                }
            }
        }

        private static void SeedDomain()
        {
            using (var context = new ApplicationDbContext())
            {
                if (!context.Senders.Any())
                {
                    context.Senders.Add(new Sender
                    {
                        Name = "Nationales Referenzzentrum",
                        Department = "Institut für Hygiene und Mikrobiologie",
                        StreetWithNumber = "Josef-Schneider-Str. 2",
                        PostalCode = "97080",
                        City = "Würzburg",
                        Phone1 = "+49 931 31-46161",
                        Email = "nrz@example.org"
                    });
                    context.SaveChanges();
                }

                if (!context.Patients.Any())
                {
                    context.Patients.Add(new Patient
                    {
                        Initials = "A.B.",
                        BirthDate = new DateTime(1985, 3, 12),
                        PostalCode = "97070",
                        City = "Würzburg"
                    });
                    context.SaveChanges();
                }

                if (!context.Sendings.Any())
                {
                    context.Sendings.Add(new Sending
                    {
                        SenderId = 1,
                        PatientId = 1,
                        SamplingDate = Sampling,
                        ReceivingDate = Receiving,
                        SenderLaboratoryNumber = "L-2020-001"
                    });
                    context.SaveChanges();
                }

                if (!context.Isolates.Any())
                {
                    context.Isolates.Add(new Isolate
                    {
                        SendingId = 1,
                        StemNumber = 1,
                        YearlySequentialIsolateNumber = 1,
                        Year = 2020
                    });
                    context.SaveChanges();
                }

                if (!context.MeningoPatients.Any())
                {
                    context.MeningoPatients.Add(new MeningoPatient
                    {
                        Initials = "C.D.",
                        BirthDate = new DateTime(1990, 7, 5),
                        PostalCode = "97070",
                        City = "Würzburg"
                    });
                    context.SaveChanges();
                }

                if (!context.MeningoSendings.Any())
                {
                    context.MeningoSendings.Add(new MeningoSending
                    {
                        SenderId = 1,
                        MeningoPatientId = 1,
                        SamplingDate = Sampling,
                        ReceivingDate = Receiving,
                        SenderLaboratoryNumber = "M-2020-001"
                    });
                    context.SaveChanges();
                }

                if (!context.MeningoIsolates.Any())
                {
                    context.MeningoIsolates.Add(new MeningoIsolate
                    {
                        MeningoSendingId = 1,
                        StemNumber = 1,
                        YearlySequentialIsolateNumber = 1,
                        Year = 2020
                    });
                    context.SaveChanges();
                }
            }
        }
    }
}
