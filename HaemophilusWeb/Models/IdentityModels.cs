using System;
using System.Data.Entity;
using HaemophilusWeb.Models.Meningo;
using Microsoft.AspNet.Identity.EntityFramework;

namespace HaemophilusWeb.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema:false)
        {
        }

        protected virtual void InitializeDatabase()
        {
            if (!Database.Exists())
            {
                Database.Initialize(true);
                SeedRoles();
            }
        }

        private void SeedRoles()
        {
            Roles.Add(new IdentityRole(DefaultRoles.Administrator));
            Roles.Add(new IdentityRole(DefaultRoles.User));
            Roles.Add(new IdentityRole(DefaultRoles.PublicHealth));
            SaveChanges();
        }

        public DbSet<Sender> Senders { get; set; }

        public DbSet<Patient> Patients { get; set; }

        public DbSet<Sending> Sendings { get; set; }

        public DbSet<Isolate> Isolates { get; set; }

        public DbSet<EpsilometerTest> EpsilometerTests { get; set; }

        public DbSet<EucastClinicalBreakpoint> EucastClinicalBreakpoints { get; set; }

        public DbSet<County> Counties { get; set; }

        public DbSet<HealthOffice> HealthOffices { get; set; }

        public DbSet<Meningo.MeningoPatient> MeningoPatients { get; set; }

        public DbSet<MeningoSending> MeningoSendings { get; set; }
    }
}