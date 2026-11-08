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

        /// <summary>
        /// Sender of the spec-verification fixture with an invasive Meningococci finding (MZ004/20), the only
        /// record the RKI, IRIS and LGA exports include (spec 003 FR-021, spec 011).
        /// </summary>
        public const string PositiveFindingSenderName = "SV-Seed-Positive";

        /// <summary>
        /// Prefix of the "Antibiotikum Details" of the seeded clinical breakpoints, so the E-Test sections of both
        /// isolate forms, the reports and the exports have breakpoints (specs 004, 005, 008, 009, 011, 017).
        /// </summary>
        public const string FixtureBreakpointPrefix = "SV-Seed";

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
        /// MZ003/20 (soft-deleted); all attributed to <see cref="FixtureSenderName"/>. Meningococci MZ004/20
        /// (active, invasive, Neisseria meningitidis serogroup W/Y) attributed to <see cref="PositiveFindingSenderName"/>.
        /// Clinical breakpoints for the standard antibiotics of both databases (see <see cref="SeedFixtureBreakpoints"/>).
        /// </summary>
        public static void SeedSpecVerificationFixtures()
        {
            using (var context = new ApplicationDbContext())
            {
                SeedFixtureBreakpoints(context);

                if (context.Senders.Any(s => s.Name == FixtureSenderName))
                {
                    return;
                }

                var sender = AddFixtureSender(context, FixtureSenderName, "Seed for 013 FR-015 / DEC-002");
                SeedFixtureSending(context, sender.SenderId, "S.A.", new DateTime(2020, 3, 10), "SEED-H-1", 2, deleted: false);
                SeedFixtureSending(context, sender.SenderId, "S.B.", new DateTime(2020, 6, 15), "SEED-H-2", 3, deleted: true);
                SeedFixtureMeningoSending(context, sender.SenderId, "S.C.", new DateTime(2020, 3, 10), "SEED-M-1", 2, deleted: false);
                SeedFixtureMeningoSending(context, sender.SenderId, "S.D.", new DateTime(2020, 6, 15), "SEED-M-2", 3, deleted: true);

                var positiveSender = AddFixtureSender(context, PositiveFindingSenderName, "Seed for 003 FR-021 / 011 positive finding");
                var positiveIsolate = SeedFixtureMeningoSending(context, positiveSender.SenderId, "S.E.",
                    new DateTime(2020, 5, 20), "SEED-M-3", 4, deleted: false, MeningoSamplingLocation.Blood);
                // Matches stem interpretation rule 8 (see MeningoIsolateInterpretationTests): meningococci found.
                positiveIsolate.GrowthOnBloodAgar = Growth.TypicalGrowth;
                positiveIsolate.GrowthOnMartinLewisAgar = Growth.TypicalGrowth;
                positiveIsolate.Oxidase = TestResult.Positive;
                positiveIsolate.Agglutination = MeningoSerogroupAgg.Auto;
                positiveIsolate.Onpg = TestResult.Negative;
                positiveIsolate.GammaGt = TestResult.Positive;
                positiveIsolate.SerogroupPcr = MeningoSerogroupPcr.WY;
                positiveIsolate.MaldiTofVitek = UnspecificTestResult.NotDetermined;
                positiveIsolate.PorAPcr = NativeMaterialTestResult.Positive;
                positiveIsolate.FetAPcr = NativeMaterialTestResult.Positive;
                positiveIsolate.PorAVr1 = "X";
                positiveIsolate.PorAVr2 = "Y";
                positiveIsolate.FetAVr = "Z";
                context.SaveChanges();
            }
        }

        /// <summary>
        /// Breakpoints for the standard antibiotics (<c>PrimaryAntibiotics_*</c>) of both databases, valid from
        /// 01.01.2024, plus an older Ampicillin version and one record "ohne Grenzwerte" per database. Thresholds
        /// are plausible test values, not authoritative EUCAST data. Idempotent.
        /// </summary>
        private static void SeedFixtureBreakpoints(ApplicationDbContext context)
        {
            if (context.EucastClinicalBreakpoints.Any(b => b.AntibioticDetails.StartsWith(FixtureBreakpointPrefix)))
            {
                return;
            }

            var current = new DateTime(2024, 1, 1);
            var breakpoints = new[]
            {
                Breakpoint(DatabaseType.Haemophilus, Antibiotic.Ampicillin, "14.0", current, 1f, 1f),
                Breakpoint(DatabaseType.Haemophilus, Antibiotic.Ampicillin, "10.0", new DateTime(2020, 1, 1), 1f, 2f),
                Breakpoint(DatabaseType.Haemophilus, Antibiotic.AmoxicillinClavulanate, "14.0", current, 2f, 2f),
                Breakpoint(DatabaseType.Haemophilus, Antibiotic.Cefotaxime, "14.0", current, 0.125f, 0.125f),
                Breakpoint(DatabaseType.Haemophilus, Antibiotic.Meropenem, "14.0", current, 0.25f, 2f),
                Breakpoint(DatabaseType.Haemophilus, Antibiotic.Azithromycin, null, null, null, null),
                Breakpoint(DatabaseType.Meningococci, Antibiotic.Benzylpenicillin, "14.0", current, 0.064f, 0.25f),
                Breakpoint(DatabaseType.Meningococci, Antibiotic.Ciprofloxacin, "14.0", current, 0.016f, 0.016f),
                Breakpoint(DatabaseType.Meningococci, Antibiotic.Rifampicin, "14.0", current, 0.25f, 0.25f),
                Breakpoint(DatabaseType.Meningococci, Antibiotic.Cefotaxime, "14.0", current, 0.125f, 0.125f),
                Breakpoint(DatabaseType.Meningococci, Antibiotic.Azithromycin, null, null, null, null)
            };
            foreach (var breakpoint in breakpoints)
            {
                context.EucastClinicalBreakpoints.Add(breakpoint);
            }

            context.SaveChanges();
        }

        private static EucastClinicalBreakpoint Breakpoint(DatabaseType validFor, Antibiotic antibiotic, string version,
            DateTime? validFrom, float? susceptible, float? resistant)
        {
            return new EucastClinicalBreakpoint
            {
                ValidFor = validFor,
                Antibiotic = antibiotic,
                AntibioticDetails = $"{FixtureBreakpointPrefix} {antibiotic} {validFor}",
                Version = version,
                ValidFrom = validFrom,
                MicBreakpointSusceptible = susceptible,
                MicBreakpointResistent = resistant
            };
        }

        private static Sender AddFixtureSender(ApplicationDbContext context, string name, string department)
        {
            var sender = new Sender
            {
                Name = name,
                Department = department,
                PostalCode = "97080",
                City = "Würzburg",
                Phone1 = "+49 931 0000-0"
            };
            context.Senders.Add(sender);
            context.SaveChanges();
            return sender;
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

        private static MeningoIsolate SeedFixtureMeningoSending(ApplicationDbContext context, int senderId, string initials,
            DateTime sampling, string senderLaboratoryNumber, int number, bool deleted,
            MeningoSamplingLocation samplingLocation = default)
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
                SamplingLocation = samplingLocation,
                Deleted = deleted
            };
            context.MeningoSendings.Add(sending);
            context.SaveChanges();

            var isolate = new MeningoIsolate
            {
                MeningoSendingId = sending.MeningoSendingId,
                StemNumber = number,
                YearlySequentialIsolateNumber = number,
                Year = 2020
            };
            context.MeningoIsolates.Add(isolate);
            context.SaveChanges();
            return isolate;
        }
    }
}
