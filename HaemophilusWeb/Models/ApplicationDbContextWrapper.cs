using System.Data.Entity;
using System.Data.Entity.Infrastructure;

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

        public DbEntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class
        {
            return wrappedContext.Entry(entity);
        }

        public int SaveChanges()
        {
            return wrappedContext.SaveChanges();
        }
    }
}