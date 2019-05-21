using HaemophilusWeb.Models;
using HaemophilusWeb.ViewModels;

namespace HaemophilusWeb.Controllers
{
    public class ReportController : ReportControllerBase<Isolate, IsolateViewModel>
    {
        public ReportController()
            : this(new ApplicationDbContextWrapper(new ApplicationDbContext()))
        {
        }

        public ReportController(IApplicationDbContext applicationDbContext) : base(applicationDbContext, new IsolateController(applicationDbContext))
        {
        }
    }
}