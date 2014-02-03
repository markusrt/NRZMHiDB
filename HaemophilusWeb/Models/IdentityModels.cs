﻿using System.Data.Entity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace HaemophilusWeb.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection")
        {
        }

        public DbSet<Sender> Senders { get; set; }

        public DbSet<Patient> Patients { get; set; }

        public DbSet<Sending> Sendings { get; set; }
    }
}