using System.Web.Mvc;
using HaemophilusWeb.Models;
using HaemophilusWeb.Services;

namespace HaemophilusWeb.Controllers
{
    public class PubMlstController : Controller
    {
        private readonly PubMlstService _service;

        public PubMlstController() : this(new PubMlstService())
        {
        }

        public PubMlstController(PubMlstService pubMlstService)
        {
            _service = pubMlstService;
        }

        [HttpPost]
        [Authorize(Roles = DefaultRoles.User)]
        public ActionResult NeisseriaIsolates(string isolateReference)
        {
            
            var isolate = _service.GetIsolateByReference(isolateReference);
            if (isolate == null)
            {
                return new HttpNotFoundResult();
            }
            return Json(isolate);
        }
    }
}