using System.Configuration;
using System.Web.Mvc;
using HaemophilusWeb.Models;
using HaemophilusWeb.Services;
using HaemophilusWeb.Utils;


namespace HaemophilusWeb.Controllers
{
    public class PubMlstController : Controller
    {
        private readonly PubMlstService[] _pubMlstServices;

        public PubMlstController() : this(new IrisPubMlstService(ConfigurationManager.AppSettings.GetIrisAuthentication()), new NeisseriaPubMlstService())
        {
        }

        public PubMlstController(params PubMlstService[] pubMlstServices)
        {
            _pubMlstServices = pubMlstServices;
        }

        [HttpPost]
        [Authorize(Roles = DefaultRoles.User)]
        public ActionResult NeisseriaIsolates(string isolateReference)
        {
            NeisseriaPubMlstIsolate isolate = null;
            foreach (var service in _pubMlstServices)
            {
                isolate = service.GetIsolateByReference(isolateReference);
                if (isolate != null)
                {
                    break;
                }
            }
            if (isolate == null)
            {
                return new HttpNotFoundResult();
            }
            return Json(isolate);
        }
    }
}
