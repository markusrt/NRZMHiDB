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

        /// <summary>
        /// Additional accounts with exactly one role each, used by the spec-verification host to check
        /// role-specific behavior (spec 021). All share <see cref="Password"/>.
        /// </summary>
        public static readonly (string UserName, string Role)[] SingleRoleUsers =
        {
            ("Standard1", DefaultRoles.User),
            ("Admin1", DefaultRoles.Administrator),
            ("Rki1", DefaultRoles.PublicHealth)
        };

        /// <summary>
        /// Sender of the spec-verification fixture: one active and one soft-deleted submission per database,
        /// both sampled in 2020, for the sender export's counts (spec 013 FR-015, backlog DEC-002).
        /// </summary>
        public const string FixtureSenderName = "SV-Seed-DEC-002";

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

                foreach (var (singleRoleUserName, role) in SingleRoleUsers)
                {
                    if (userManager.FindByName(singleRoleUserName) != null)
                    {
                        continue;
                    }

                    var user = new ApplicationUser { UserName = singleRoleUserName };
                    var result = userManager.Create(user, Password);
                    if (!result.Succeeded)
                    {
                        throw new InvalidOperationException(
                            $"Failed to create test user {singleRoleUserName}: " + string.Join("; ", result.Errors));
                    }

                    userManager.AddToRole(user.Id, role);
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

        /// <summary>
        /// Data that only the spec-verification host seeds, on top of <see cref="Seed"/>, so the gold-master
        /// screenshots keep their single-record lists. Idempotent.
        /// Haemophilus: KL002/20 (active), KL003/20 (soft-deleted); Meningococci: MZ002/20 (active),
        /// MZ003/20 (soft-deleted); all attributed to <see cref="FixtureSenderName"/>.
        /// </summary>
        public static void SeedSpecVerificationFixtures()
        {
            using (var context = new ApplicationDbContext())
            {
                if (context.Senders.Any(s => s.Name == FixtureSenderName))
                {
                    return;
                }

                var sender = new Sender
                {
                    Name = FixtureSenderName,
                    Department = "Seed for 013 FR-015 / DEC-002",
                    PostalCode = "97080",
                    City = "Würzburg",
                    Phone1 = "+49 931 0000-0"
                };
                context.Senders.Add(sender);
                context.SaveChanges();

                SeedFixtureSending(context, sender.SenderId, "S.A.", new DateTime(2020, 3, 10), "SEED-H-1", 2, deleted: false);
                SeedFixtureSending(context, sender.SenderId, "S.B.", new DateTime(2020, 6, 15), "SEED-H-2", 3, deleted: true);
                SeedFixtureMeningoSending(context, sender.SenderId, "S.C.", new DateTime(2020, 3, 10), "SEED-M-1", 2, deleted: false);
                SeedFixtureMeningoSending(context, sender.SenderId, "S.D.", new DateTime(2020, 6, 15), "SEED-M-2", 3, deleted: true);
            }
        }

        private static void SeedFixtureSending(ApplicationDbContext context, int senderId, string initials,
            DateTime sampling, string senderLaboratoryNumber, int number, bool deleted)
        {
            var patient = new Patient { Initials = initials, BirthDate = new DateTime(1990, 1, number), PostalCode = "97082", City = "Würzburg" };
            context.Patients.Add(patient);
            context.SaveChanges();

            var sending = new Sending
            {
                SenderId = senderId,
                PatientId = patient.PatientId,
                SamplingDate = sampling,
                ReceivingDate = sampling.AddDays(2),
                SenderLaboratoryNumber = senderLaboratoryNumber,
                Deleted = deleted
            };
            context.Sendings.Add(sending);
            context.SaveChanges();

            context.Isolates.Add(new Isolate
            {
                SendingId = sending.SendingId,
                StemNumber = number,
                YearlySequentialIsolateNumber = number,
                Year = 2020
            });
            context.SaveChanges();
        }

        private static void SeedFixtureMeningoSending(ApplicationDbContext context, int senderId, string initials,
            DateTime sampling, string senderLaboratoryNumber, int number, bool deleted)
        {
            var patient = new MeningoPatient { Initials = initials, BirthDate = new DateTime(1990, 2, number), PostalCode = "97082", City = "Würzburg" };
            context.MeningoPatients.Add(patient);
            context.SaveChanges();

            var sending = new MeningoSending
            {
                SenderId = senderId,
                MeningoPatientId = patient.PatientId,
                SamplingDate = sampling,
                ReceivingDate = sampling.AddDays(2),
                SenderLaboratoryNumber = senderLaboratoryNumber,
                Deleted = deleted
            };
            context.MeningoSendings.Add(sending);
            context.SaveChanges();

            context.MeningoIsolates.Add(new MeningoIsolate
            {
                MeningoSendingId = sending.MeningoSendingId,
                StemNumber = number,
                YearlySequentialIsolateNumber = number,
                Year = 2020
            });
            context.SaveChanges();
        }
    }
}
