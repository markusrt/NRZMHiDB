using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HaemophilusWeb.Models;
using HaemophilusWeb.Tools;

namespace HaemophilusWeb.Controllers
{
    public class RkiMatchController : Controller
    {
        private readonly IApplicationDbContext db = new ApplicationDbContextWrapper(new ApplicationDbContext());

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(HttpPostedFileBase file)
        {
            try
            {
                if (file.ContentLength > 0)
                {
                    var path = Path.GetTempFileName();
                    file.SaveAs(path);
                    var importer = new RkiMatchImporter(path, db);
                    importer.ImportMatches();
                }
                ViewBag.Message = "File Uploaded Successfully!!";
                return View();
            }
            catch(Exception e)
            {
                ViewBag.Message = "File upload failed: " + e.Message;
                return View();
            }
        }
    }
}