using HaemophilusWeb.Models;
using HaemophilusWeb.Models.Meningo;
using HaemophilusWeb.ViewModels;

namespace HaemophilusWeb.Controllers
{
    public class MeningoReportController : ReportControllerBase<MeningoIsolate, MeningoIsolateViewModel>
    {
        public MeningoReportController()
            : this(new ApplicationDbContextWrapper(new ApplicationDbContext()))
        {
        }

        public MeningoReportController(IApplicationDbContext applicationDbContext) : base(applicationDbContext, new MeningoIsolateController(applicationDbContext))
        {
        }
    }
}