using HaemophilusWeb.Models;
using HaemophilusWeb.Models.Meningo;
using HaemophilusWeb.ViewModels;

namespace HaemophilusWeb.Controllers
{
    public class MeningoReportController : ReportControllerBase<MeningoIsolate, MeningoIsolateViewModel>
    {
        private const string ReportTemplatesPath = "~/ReportTemplates/Meningo";

        public MeningoReportController()
            : this(new ApplicationDbContextWrapper(new ApplicationDbContext()))
        {
        }

        public MeningoReportController(IApplicationDbContext applicationDbContext) : base(applicationDbContext, new MeningoIsolateController(applicationDbContext), ReportTemplatesPath)
        {
        }
    }
}