﻿using System;
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
            new Change(new DateTime(2014, 9, 29), "Erweiterte Erfassung von Einsendungen I", ChangeType.Feature),
            new Change(new DateTime(2014, 9, 25), "Umsortierung in der Isolat-Bearbeitungsmaske", ChangeType.Design),
            new Change(new DateTime(2014, 9, 25), "Speichern der Bearbeitungsposition in der Einsendungsliste",
                ChangeType.Feature),
            new Change(new DateTime(2014, 9, 23), "Erweiterte Erfassung von Isolaten V", ChangeType.Feature),
            new Change(new DateTime(2014, 9, 22), "Erweiterte Erfassung von Patienten III", ChangeType.Feature),
            new Change(new DateTime(2014, 8, 31), "Erweiterung der Erfassung von Isolaten IV", ChangeType.Feature),
            new Change(new DateTime(2014, 8, 30), "Erweiterte Erfassung von Patienten II", ChangeType.Feature),
            new Change(new DateTime(2014, 8, 10), "Komfortable Erfassung von Einsendung /Patient", ChangeType.Feature),
            new Change(new DateTime(2014, 7, 29), "Erweiterung der Erfassung von Isolaten III", ChangeType.Feature),
            new Change(new DateTime(2014, 6, 19), "Sortierung der Einsendungsliste", ChangeType.Feature),
            new Change(new DateTime(2014, 6, 17), "Sortierung der Einsenderlisten", ChangeType.Feature),
            new Change(new DateTime(2014, 6, 14), "Erstellung von Befunden zu Isolaten", ChangeType.Feature),
            new Change(new DateTime(2014, 6, 14), "Speicherung von Befundsvorlagen", ChangeType.Feature),
            new Change(new DateTime(2014, 6, 10), "Erweiterung der Erfassung von Isolaten II", ChangeType.Feature),
            new Change(new DateTime(2014, 5, 30), "Automatische Zuweisung einer Stammnummer", ChangeType.Feature),
            new Change(new DateTime(2014, 5, 15), "Layoutänderung im Isolateditor und Einsendungseditor",
                ChangeType.Design),
            new Change(new DateTime(2014, 5, 6), "Erweiterung der Erfassung von Isolaten", ChangeType.Feature),
            new Change(new DateTime(2014, 5, 5), "Editierbare Stammnummer", ChangeType.Feature),
            new Change(new DateTime(2014, 4, 15), "Erfassung von Isolaten", ChangeType.Feature),
            new Change(new DateTime(2014, 3, 18), "Einfache Erfassung von Einsendung/Patient", ChangeType.Feature),
            new Change(new DateTime(2014, 2, 16), "Zuweisung einer Stammnummer", ChangeType.Feature),
            new Change(new DateTime(2014, 1, 11), "Einfache Erfassung von Einsendungen", ChangeType.Feature),
            new Change(new DateTime(2014, 1, 8), "Geänderte Felder für Einsender (Straße, Postleitzahl, Stadt)",
                ChangeType.Feature),
            new Change(new DateTime(2014, 1, 6),
                "Erweiterte Erfassung von Patienten (Impfdatum, andere klinische Angaben)", ChangeType.Feature),
            new Change(new DateTime(2014, 1, 5), "Postleitzahl-Suche bei der Patientenerfassung", ChangeType.Feature),
            new Change(new DateTime(2013, 11, 25), "Patient anlegen und bearbeiten", ChangeType.Feature),
            new Change(new DateTime(2013, 11, 13), "Layout der Formulare", ChangeType.Design),
            new Change(new DateTime(2013, 11, 13), "Einsender anlegen und bearbeiten", ChangeType.Feature),
            new Change(new DateTime(2013, 11, 12), "Versionsinformation anzeigen", ChangeType.Feature),
            new Change(new DateTime(2013, 11, 11), "Einführung des Changelogs", ChangeType.Feature),
            new Change(new DateTime(2013, 11, 10), "Anpassung des Layouts und der Farben", ChangeType.Design),
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