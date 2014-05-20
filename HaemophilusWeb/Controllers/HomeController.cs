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
            new Change(new DateTime(2014,5,15), "Layoutänderung im Isolateditor und Einsendungseditor", ChangeType.Design ),
            new Change(new DateTime(2014,5,6), "Erweiterung der Erfassung von Isolaten", ChangeType.Feature ),
            new Change(new DateTime(2014,5,5), "Editierbare Stammnummer", ChangeType.Feature ),
            new Change(new DateTime(2014,4,15), "Erfassung von Isolaten", ChangeType.Feature ),
            new Change(new DateTime(2014,3,18), "Einfache Erfassung von Einsendung/Patient", ChangeType.Feature ),
            new Change(new DateTime(2014,2,16), "Zuweisung einer Stammnummer", ChangeType.Feature ),
            new Change(new DateTime(2014,1,11), "Einfache Erfassung von Einsendungen", ChangeType.Feature ),
            new Change(new DateTime(2014,1,8), "Geänderte Felder für Einsender (Straße, Postleitzahl, Stadt)", ChangeType.Feature ),
            new Change(new DateTime(2014,1,6), "Erweiterte Erfassung von Patienten (Impfdatum, andere klinische Angaben)", ChangeType.Feature ),
            new Change(new DateTime(2014,1,5), "Postleitzahl-Suche bei der Patientenerfassung", ChangeType.Feature ),
            new Change(new DateTime(2013,11,25), "Patient anlegen und bearbeiten", ChangeType.Feature ),
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