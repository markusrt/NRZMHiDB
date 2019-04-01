using System;
using System.Data.Entity;
using HaemophilusWeb.Models.Meningo;

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

        public IDbSet<Sender> Senders => wrappedContext.Senders;

        public IDbSet<Patient> Patients => wrappedContext.Patients;

        public IDbSet<Sending> Sendings => wrappedContext.Sendings;

        public IDbSet<Isolate> Isolates => wrappedContext.Isolates;

        public IDbSet<County> Counties => wrappedContext.Counties;

        public IDbSet<HealthOffice> HealthOffices => wrappedContext.HealthOffices;

        public IDbSet<EpsilometerTest> EpsilometerTests => wrappedContext.EpsilometerTests;

        public IDbSet<EucastClinicalBreakpoint> EucastClinicalBreakpoints => wrappedContext.EucastClinicalBreakpoints;

        public IDbSet<MeningoPatient> MeningoPatients => wrappedContext.MeningoPatients;

        public IDbSet<MeningoSending> MeningoSendings => wrappedContext.MeningoSendings;

        public IDbSet<MeningoIsolate> MeningoIsolates => wrappedContext.MeningoIsolates;

        public IDbSet<NeisseriaPubMlstIsolate> NeisseriaPubMlstIsolates => wrappedContext.NeisseriaPubMlstIsolate;

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
                catch (Exception e)
                {
                    transaction.Rollback();
                    throw e;
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