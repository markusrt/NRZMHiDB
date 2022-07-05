using System.Linq;
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

        protected override void IgnoreAntibioticsFromReport(MeningoIsolateViewModel isolateViewModel)
        {
            isolateViewModel.EpsilometerTestViewModels = isolateViewModel.EpsilometerTestViewModels
                .Where(e => e.Antibiotic != Antibiotic.Azithromycin).ToList();
        }
    }
}