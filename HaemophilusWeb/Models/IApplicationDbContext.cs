using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;

namespace HaemophilusWeb.Models
{
    public interface IApplicationDbContext : IDisposable
    {
        IDbSet<Sender> Senders { get; }

        IDbSet<Patient> Patients { get; }

        IDbSet<Sending> Sendings { get; }

        IDbSet<Isolate> Isolates { get; }

        DbEntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class;

        int SaveChanges();

        void WrapInTransaction(Action action);
    }
}