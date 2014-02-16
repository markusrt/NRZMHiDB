using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using HaemophilusWeb.Mocks;

namespace HaemophilusWeb.Models
{
    public class ApplicationDbContextMock : IApplicationDbContext
    {
        public readonly InMemoryDbSet<Patient> PatientDbSet = new InMemoryDbSet<Patient>(true);
        public readonly InMemoryDbSet<Sender> SenderDbSet = new InMemoryDbSet<Sender>(true);
        public readonly InMemoryDbSet<Sending> SendingDbSet = new MockDbSet<Sending>(true);
        public readonly InMemoryDbSet<Isolate> IsolatesDbSet = new InMemoryDbSet<Isolate>(true);

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

        public IDbSet<Isolate> Isolates
        {
            get { return IsolatesDbSet; }
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

    public class MockDbSet<T> : InMemoryDbSet<Sending>
    {
        public MockDbSet(bool clearDownExistingData) : base(clearDownExistingData)
        {
        }

        public override Sending Find(params object[] keyValues)
        {
            return this.FirstOrDefault(s => keyValues.Contains(s.SendingId));
        }
    }
}