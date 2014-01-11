using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using HaemophilusWeb.Mocks;

namespace HaemophilusWeb.Models
{
    public class ApplicationDbContextMock : IApplicationDbContext
    {
        public readonly InMemoryDbSet<Patient> PatientDbSet = new InMemoryDbSet<Patient>(true);
        public readonly InMemoryDbSet<Sender> SenderDbSet = new InMemoryDbSet<Sender>(true);
        public readonly InMemoryDbSet<Sending> SendingDbSet = new InMemoryDbSet<Sending>(true);

        public IDbSet<Sender> Senders
        {
            get { return SenderDbSet; }
        }

        public IDbSet<Patient> Patients
        {
            get { return PatientDbSet; }
        }

        public IDbSet<Sending> Sendings
        {
            get { return SendingDbSet; }
        }

        public DbEntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class
        {
            return null;
        }

        public int SaveChanges()
        {
            return 0;
        }

        public void Dispose()
        {
        }
    }
}