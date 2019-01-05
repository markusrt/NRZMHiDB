using System;
using System.Data.Entity;
using HaemophilusWeb.Models.Meningo;

namespace HaemophilusWeb.Models
{
    public interface IApplicationDbContext : IDisposable
    {
        IDbSet<Sender> Senders { get; }

        IDbSet<Patient> Patients { get; }

        IDbSet<Sending> Sendings { get; }

        IDbSet<Isolate> Isolates { get; }

        IDbSet<County> Counties { get; }

        IDbSet<HealthOffice> HealthOffices { get; }

        IDbSet<EpsilometerTest> EpsilometerTests { get; }

        IDbSet<EucastClinicalBreakpoint> EucastClinicalBreakpoints { get; }

        IDbSet<MeningoPatient> MeningoPatients { get; }

        IDbSet<MeningoSending> MeningoSendings { get; }

        IDbSet<MeningoIsolate> MeningoIsolates { get; }

        void MarkAsModified<TEntity>(TEntity entity) where TEntity : class;

        int SaveChanges();

        void WrapInTransaction(Action action);

        void PerformWithoutSaveValidation(Action action);
    }
}