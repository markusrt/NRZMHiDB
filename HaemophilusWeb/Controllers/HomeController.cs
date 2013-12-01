using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using HaemophilusWeb.Models;

namespace HaemophilusWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly List<Change> changeLog = new List<Change>
        {
            new Change(new DateTime(2013,11,25), "Patient anglegen und bearbeiten", ChangeType.Feature ),
            new Change(new DateTime(2013,11,13), "Layout der Formulare", ChangeType.Design ),
            new Change(new DateTime(2013,11,13), "Einsender anlegen und bearbeiten", ChangeType.Feature ),
            new Change(new DateTime(2013,11,12), "Versionsinformation anzeigen", ChangeType.Feature ),
            new Change(new DateTime(2013,11,11), "Einführung des Changelogs", ChangeType.Feature ),
            new Change(new DateTime(2013,11,10), "Anpassung des Layouts und der Farben", ChangeType.Design ),
        }; 

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            return View(changeLog.OrderByDescending(c => c.Date));
        }
    }
}