using System.Data.Entity;
using HaemophilusWeb.Models;
using HaemophilusWeb.Models.Meningo;

namespace HaemophilusWeb.Controllers
{
    public class MeningoSendingController : SendingControllerBase<MeningoSending, MeningoPatient>
    {
        public MeningoSendingController() : this(new ApplicationDbContextWrapper(new ApplicationDbContext()))
        {
        }

        public MeningoSendingController(IApplicationDbContext applicationDbContext) : base(applicationDbContext)
        {
        }

        protected override IDbSet<Isolate> IsolateDbSet()
        {
            return db.Isolates;
        }

        protected override IDbSet<MeningoSending> SendingDbSet()
        {
            return db.MeningoSendings;
        }

        protected override IDbSet<MeningoPatient> PatientDbSet()
        {
            return db.MeningoPatients;
        }
    }
}
