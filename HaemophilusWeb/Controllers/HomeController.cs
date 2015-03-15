using System;
using System.Collections.Generic;
using System.Web.Mvc;
using HaemophilusWeb.Models;

namespace HaemophilusWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly ChangeLog changeLog = new ChangeLog(
            new List<Change>
            {
                new Change(DateTime.MinValue, "Absicherung des Servers mit HTTPS", ChangeType.Feature),
                new Change(DateTime.MinValue, "Suche von Einsendungen nach Einsender-Labornummer und -PLZ",
                    ChangeType.Feature),
                new Change(DateTime.MinValue, "Abfrage über Isolate", ChangeType.Feature),
                new Change(DateTime.MinValue, "Erweiterte Erfassung von Isolaten VII (Wachstum und Art des Wachstums)",
                    ChangeType.Feature),
                new Change(DateTime.MinValue, "Unterstützung für doppelte Einsendungen eines Patienten",
                    ChangeType.Feature),
            },
            new List<Change>
            {
                new Change(new DateTime(2015, 3, 15),
                    "Markierung von Einsendungen für die bereits ein Befund erstellt wurde",
                    ChangeType.Feature),
                new Change(new DateTime(2015, 3, 15), "Auditierung der Befunderstellung", ChangeType.Feature),
                new Change(new DateTime(2015, 2, 25), "Leerzeichen bei H.xxx einfügen (-> H. xxx)", ChangeType.Bug),
                new Change(new DateTime(2015, 2, 25), "Login mit einfachem Benutzernamen und Passwort",
                    ChangeType.Feature),
                new Change(new DateTime(2015, 2, 23), "Labor Export Modul", ChangeType.Feature),
                new Change(new DateTime(2015, 2, 13), "RKI Export Modul", ChangeType.Feature),
                new Change(new DateTime(2015, 1, 31), "Update der Befundvorlagen", ChangeType.Feature),
                new Change(new DateTime(2015, 1, 31), "Hinzufügen der EUCAST Grenzwerte für 2015", ChangeType.Feature),
                new Change(new DateTime(2015, 1, 31), "Automatische Selektion der EUCAST Interpretation",
                    ChangeType.Feature),
                new Change(new DateTime(2015, 1, 31), "Hib Impfung - Datum ist optional", ChangeType.Feature),
                new Change(new DateTime(2015, 1, 31), "Hinzufügen von Beta-Lactamase zu Befundauszug",
                    ChangeType.Feature),
                new Change(new DateTime(2015, 1, 30), "Import der Daten von 1/2008 bis 8/2014", ChangeType.Feature),
                new Change(new DateTime(2015, 1, 29),
                    "Falsche Interpretation von Kommazahlen bei Antibiotika-Resistenzen",
                    ChangeType.Bug),
                new Change(new DateTime(2015, 1, 15), "Wechsel von Isolat zu Befund und umgekehrt", ChangeType.Feature),
                new Change(new DateTime(2015, 1, 15),
                    "Vorschau des Interpretationssatzes auf der Befunderstellungs-Maske", ChangeType.Design),
                new Change(new DateTime(2015, 1, 15),
                    "Umsetzung der aktualisierten Interpretationssatz-Vorschrift bei welcher Serotyp-PCR auch 'n.d.' sein darf",
                    ChangeType.Feature),
                new Change(new DateTime(2015, 1, 15), "Korrektur der fehlerhaften Feld-Referenzen in Befundvorlagen",
                    ChangeType.Bug),
                new Change(new DateTime(2014, 11, 10), "Aktualisierung der Befundvorlagen", ChangeType.Feature),
                new Change(new DateTime(2014, 10, 2), "Update der Befundvorlagen und einfügen der Einsender-Adresse",
                    ChangeType.Feature),
                new Change(new DateTime(2014, 9, 29), "Erweiterte Erfassung von Einsendungen I", ChangeType.Feature),
                new Change(new DateTime(2014, 9, 25), "Umsortierung in der Isolat-Bearbeitungsmaske", ChangeType.Design),
                new Change(new DateTime(2014, 9, 25), "Speichern der Bearbeitungsposition in der Einsendungsliste",
                    ChangeType.Feature),
                new Change(new DateTime(2014, 9, 23), "Erweiterte Erfassung von Isolaten V", ChangeType.Feature),
                new Change(new DateTime(2014, 9, 22), "Erweiterte Erfassung von Patienten III", ChangeType.Feature),
                new Change(new DateTime(2014, 8, 31), "Erweiterung der Erfassung von Isolaten IV", ChangeType.Feature),
                new Change(new DateTime(2014, 8, 30), "Erweiterte Erfassung von Patienten II", ChangeType.Feature),
                new Change(new DateTime(2014, 8, 10), "Komfortable Erfassung von Einsendung /Patient",
                    ChangeType.Feature),
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
            });

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            return View(changeLog);
        }
    }
}