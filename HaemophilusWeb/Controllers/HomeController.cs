using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using HaemophilusWeb.Models;

namespace HaemophilusWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly ChangeLog changeLog = new ChangeLog(
            new List<Change>
            {
                new Change(DateTime.MinValue, "Automatische Bestimmung des zugehörigen Gesundheitsamtes",
                    ChangeType.Feature),
                new Change(DateTime.MinValue, "Spezieller Zugang für Robert Koch Institut", ChangeType.Feature),
                new Change(DateTime.MinValue, "Automatische Bestimmung des zugehörigen Gesundheitsamtes",
                    ChangeType.Feature),
                new Change(DateTime.MinValue, "<del>Auditierung von Zugriffen</del>", ChangeType.Feature),
                new Change(DateTime.MinValue, "<del>Abfrage über Isolate</del>", ChangeType.Feature)
            },
            new List<Change>
            {
                new Change(new DateTime(2015, 10, 30), "Befundvorlagen für Fax-Versand", ChangeType.Feature),
                new Change(new DateTime(2015, 8, 31),
                    "<p>Unterstützung für doppelte Einsendungen eines Patienten</p>" +
                    "<p>Falls ein Patient mit den selben Initialen, Postleitzahl und Geburtsdatum bereits " +
                    "im System gespeichert ist, erscheint folgender Dialog:<p>" +
                    "<img class='img-rounded img-responsive' src='~/Images/changes/20150831_1.png'/>" +
                    "<p>Je nach Auswahl des Anwenders wird entweder ein zweiter Patient angelegt oder der " +
                    "bestehende Patient wird erweitert.</p>",
                    ChangeType.Feature),
                new Change(new DateTime(2015, 8, 22), 
                    "<p>Sortierung von gelöschten Einsendungen und Einsendern</p>" +
                    "<p>Die Listen der gelöschten Einsendungen und Einsender sind nach allen Feldern sortierbar.</p>", 
                    ChangeType.Feature),
                new Change(new DateTime(2015, 7, 28), 
                    "<p>Löschen von Isolaten und Einsendungen</p>" +
                    "<p>Analog zum Löschen von Einsendern ist es jetzt möglich Einsendungen als gelöscht zu markieren. " +
                    "In der Bearbeitungsmaske für Einsendungen gibt es einen neuen Knopf zum Löschen</p>" +
                    "<p>Gelöschte Einsendungen können über 'Administration→Gelöschte Einsendungen' wiederhergestellt werden</p>", ChangeType.Feature),
                new Change(new DateTime(2015, 7, 15),
                    "<p>Löschen von Einsendern</p>" +
                    "<p>Am Ende der Bearbeitungsmaske für Einsender gibt es einen neuen Knopf zum Löschen.<p>" +
                    "<img class='img-rounded img-responsive' src='~/Images/changes/20150715_1.png'/>" +
                    "<p>Falls einem Einsender noch Einsendungen zugeordnet sind, so ist ein Löschen nicht möglich. In dem Fall muss der Anwender die betroffenen Einsendungen erst einem anderen Einsender zuordnen.</p>" +
                    "<img class='img-rounded img-responsive' src='~/Images/changes/20150715_2.png'/>" +
                    "<p>Einsender, von denen keine Einsendungen mehr existieren können nach einer Sicherheitsabfrage gelöscht werden.<p>" +
                    "<img class='img-rounded img-responsive' src='~/Images/changes/20150715_3.png'/>" +
                    "<p>Gelöschte Einsender können über das Menü 'Administration' jederzeit wiederhergestellt werden</p>" +
                    "<img class='img-rounded img-responsive' src='~/Images/changes/20150715_4.png'/>" +
                    "<img class='img-rounded img-responsive' src='~/Images/changes/20150715_5.png'/>",
                    ChangeType.Feature),
                new Change(new DateTime(2015, 6, 30),
                    "<p>Erweiterte Erfassung von Isolaten VII (Wachstum und Art des Wachstums)</p>" +
                    "<p>Es ist jetzt möglich bei der Isolat-Maske das Wachstum und die Art des Wachstums auszuwählen. Inhaltlich ist es so wie wir es Ende letzten Jahres festgelegt hatten. Ich habe die beiden neuen Felder auch in den Labor-Export mit aufgenommen.</p>" +
                    "<img class='img-rounded img-responsive' src='~/Images/changes/20150630.png'/>" +
                    "<p>Alle bisherigen Datensätze wurden mit 'keine Angabe' initialisiert.</p>",
                    ChangeType.Feature),
                new Change(new DateTime(2015, 6, 29),
                    "Erweiterte Suche von Einsendungen nach Geburtsdatum, Initialien, Patient PLZ, Einsender-Labornummer und Einsender PLZ",
                    ChangeType.Feature),
                new Change(new DateTime(2015, 5, 31), "Suche von Einsendungen nach Einsender-Labornummer und -PLZ",
                    ChangeType.Feature),
                new Change(new DateTime(2015, 5, 30), "Ausgabe aller Felder beim Labor-Export", ChangeType.Feature),
                new Change(new DateTime(2015, 5, 28), "Warnung bei diskrepanten Ergebnissen vor Befunderstellung",
                    ChangeType.Feature),
                new Change(new DateTime(2015, 5, 19), "Update der Befundvorlagen (Adresse, Layout, Telefon, Url)",
                    ChangeType.Feature),
                new Change(new DateTime(2015, 5, 5), "Umstellung der Geodaten-Abfrage auf HTTPS", ChangeType.Bug),
                new Change(new DateTime(2015, 4, 29), "Absicherung des Servers mit HTTPS", ChangeType.Feature),
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

        public ActionResult Change(DateTime? id)
        {
            var change = changeLog.PreviousChanges.FirstOrDefault(c => c.Date.Equals(id));
            if (change == null)
            {
                return HttpNotFound();
            }
            return View(change);
        }
    }
}