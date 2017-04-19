using System;
using System.Data.Entity;

namespace HaemophilusWeb.Models
{
    public class ApplicationDbContextWrapper : IApplicationDbContext
    {
        private readonly ApplicationDbContext wrappedContext;

        public ApplicationDbContextWrapper(ApplicationDbContext contextToWrap)
        {
            wrappedContext = contextToWrap;
        }

        public void Dispose()
        {
            wrappedContext.Dispose();
        }

        public IDbSet<Sender> Senders
        {
            get { return wrappedContext.Senders; }
        }

        public IDbSet<Patient> Patients
        {
            get { return wrappedContext.Patients; }
        }

        public IDbSet<Sending> Sendings
        {
            get { return wrappedContext.Sendings; }
        }

        public IDbSet<Isolate> Isolates
        {
            get { return wrappedContext.Isolates; }
        }

        public IDbSet<County> Counties
        {
            get { return wrappedContext.Counties; }
        }

        public IDbSet<HealthOffice> HealthOffices
        {
            get { return wrappedContext.HealthOffices; }
        }

        public IDbSet<EpsilometerTest> EpsilometerTests
        {
            get { return wrappedContext.EpsilometerTests; }
        }

        public IDbSet<EucastClinicalBreakpoint> EucastClinicalBreakpoints
        {
            get { return wrappedContext.EucastClinicalBreakpoints; }
        }

        public void MarkAsModified<TEntity>(TEntity entity) where TEntity : class
        {
            wrappedContext.Entry(entity).State = EntityState.Modified;
        }

        public int SaveChanges()
        {
            return wrappedContext.SaveChanges();
        }

        public void WrapInTransaction(Action action)
        {
            using (var transaction = wrappedContext.Database.BeginTransaction())
            {
                try
                {
                    action();
                    SaveChanges();
                    transaction.Commit();
                }
                catch (Exception)
                {
                    transaction.Rollback();
                }
            }
        }

        public void PerformWithoutSaveValidation(Action action)
        {
            wrappedContext.Configuration.ValidateOnSaveEnabled = false;
            action();
            wrappedContext.Configuration.ValidateOnSaveEnabled = true;
        }
    }
}