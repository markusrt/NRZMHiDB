using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using HaemophilusWeb.Models;

namespace HaemophilusWeb.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        private readonly ChangeLog changeLog = new ChangeLog(
            new List<Change>
            {
                new Change(DateTime.MinValue, "Umstrukturierungen für Meningokokken", ChangeType.Feature),
                new Change(DateTime.MinValue, "Entwickler Dokumentation", ChangeType.Feature),
                new Change(DateTime.MinValue, "<del>Auditierung von Zugriffen</del>", ChangeType.Feature),
                new Change(DateTime.MinValue, "<del>Abfrage über Isolate</del>", ChangeType.Feature)
            },
            new List<Change>
            {
                new Change(new DateTime(2020, 02, 03), "Fehler bei Labor Export (Haemophilus)", ChangeType.Bug),
                new Change(new DateTime(2020, 01, 21, 23, 15, 0), "Eingaben für siaA, ctrA und cnl dürfen auch dann gemacht werden, wenn kein Wachstum auf Agar erfolgt ist.", ChangeType.Feature),
                new Change(new DateTime(2020, 01, 13, 23, 15, 0), "Hinweis auf Genomsequenzierung für Befunde vitaler Stämme", ChangeType.Feature),
                new Change(new DateTime(2020, 01, 13, 22, 45, 0), "Update Befunde und Interpretation für Nativmaterialien", ChangeType.Feature),
                new Change(new DateTime(2020, 01, 13, 22, 0, 0), "Meningokokken Einsendungen können ohne Postleitzahl und Entnahmedatum gespeichert werden", ChangeType.Feature),
                new Change(new DateTime(2020, 01, 09, 22, 0, 0), "16S rDNA \"Negativ\" wird jetzt korrekt gespeichert", ChangeType.Bug),
                new Change(new DateTime(2020, 01, 03), "Verbesserung bei Befunden: Faxvorlage, 4-fache Resistenztestung, keine Interpretation", ChangeType.Bug),
                new Change(new DateTime(2020, 01, 01, 10, 0, 0), "Zusammenführung der Meningokokken und Haemophilus Version", ChangeType.Feature),
                new Change(new DateTime(2020, 01, 01, 0, 0, 0), "Erweiterung Briefkopf für Meningokokken Befundvorlagen", ChangeType.Feature),
                new Change(new DateTime(2019, 12, 02, 0, 0, 0), "Bei Meningokokken Nativmaterial wird keine Stamnummer vergeben", ChangeType.Feature),
                new Change(new DateTime(2019, 12, 02, 0, 0, 0), "Eingabefelder für siaA, ctrA und cnl bei Meningokokken", ChangeType.Feature),
                new Change(new DateTime(2019, 11, 17, 0, 0, 0), "Eingabefelder für Standard Antibiotika bei Meningokokken", ChangeType.Feature),
                new Change(new DateTime(2019, 11, 16, 23, 45, 0), "Entfernen des Feldes api NH", ChangeType.Feature),
                new Change(new DateTime(2019, 11, 16, 23, 15, 0), "Cnl wird unter Serogruppen-PCR einsortiert", ChangeType.Feature),
                new Change(new DateTime(2019, 11, 16, 23, 00, 0), "Reihenfolge von ONPG und gamma-GT vertauscht", ChangeType.Feature),
                new Change(new DateTime(2019, 11, 16, 22, 30, 0), "Land steht bei neuen Einsendungen jetzt immer auf \"Deutschland\"", ChangeType.Bug),
                new Change(new DateTime(2019, 11, 07), "Die Angabe von Serogruppen-PCR ist auch bei nicht angewachsenen Stämmen möglich", ChangeType.Feature),
                new Change(new DateTime(2019, 10, 29, 23, 0, 0), "Meningokokken, cswy-PCR ist optional für Interpretationssatz 1 und 2", ChangeType.Feature),
                new Change(new DateTime(2019, 10, 29, 22, 0, 0), "Untersuchungsbefund für Meningokokken Nativmaterial, Interpretationssätze 13-19", ChangeType.Feature),
                new Change(new DateTime(2019, 10, 29, 20, 0, 0), "Meningokokken, Berücksichtigung von \"inhibitorisch\" bei den Interpretationssätzen 6-12.", ChangeType.Feature),
                new Change(new DateTime(2019, 09, 23, 22, 0, 0), "Meningokokken, Kein Wachstum auf Agar darf nicht den Wert Nativmaterial überschreiben", ChangeType.Bug),
                new Change(new DateTime(2019, 09, 23, 20, 0, 0), "Meningokokken, Eingabefeld für das Land bei Einsendungen", ChangeType.Feature),
                new Change(new DateTime(2019, 08, 15, 22, 0, 0), "Meningokokken, Einsendung->Material: Kein Wachstum wird automatisch gesetzt wenn das Isolat nicht angewachsen ist", ChangeType.Feature),
                new Change(new DateTime(2019, 08, 15, 21, 0, 0), "Untersuchungsbefund für Meningokokken Nativmaterial, Regel 10-12", ChangeType.Feature),
                new Change(new DateTime(2019, 08, 14), "Untersuchungsbefund für Meningokokken Nativmaterial, Regel 1-9", ChangeType.Feature),
                new Change(new DateTime(2019, 08, 11), "Unterstützung von Befunden ohne Empfindlichkeitstestung", ChangeType.Feature),
                new Change(new DateTime(2019, 07, 30), "Untersuchungsbefund für Meningokokken mit allen Interpretationen für Stämme", ChangeType.Feature),
				new Change(new DateTime(2019, 07, 5), "Ergänzung des Hinweis auf Meldepflicht bei NTHi Teilbefunden.", ChangeType.Bug),
                new Change(new DateTime(2019, 06, 17), "Untersuchungsbefund für Meningokokken mit Teilinterpretation", ChangeType.Feature),
                new Change(new DateTime(2019, 06, 08), "Untersuchungsbefund für Meningokokken", ChangeType.Feature),
                new Change(new DateTime(2019, 05, 19), "Import der Meningokokken Access Datenbank für 2019", ChangeType.Feature),
                new Change(new DateTime(2019, 03, 26), "Erweiterung der Interpretationssätze um Einsendungen die kein Haemophilus influenzae sind", ChangeType.Feature),
                new Change(new DateTime(2019, 01, 29), "Präfix MZ/KL für Labornummern und DE/H für Stammnummern", ChangeType.Feature),
                new Change(new DateTime(2019, 01, 23), "Überarbeitung Molekulare Typisierung für Meningokokken", ChangeType.Feature),
                new Change(new DateTime(2019, 01, 22), "Maske Molekulare Typisierung für Meningokokken", ChangeType.Feature),
                new Change(new DateTime(2019, 01, 19, 21, 0, 0), "Erweiterung der EUCAST Werte um Meningokokken und Haemophilus zu verwalten", ChangeType.Feature),
                new Change(new DateTime(2019, 01, 18, 21, 0, 0), "Überarbeitung von Isolaten (Meningokokken)", ChangeType.Feature),
                new Change(new DateTime(2019, 01, 16, 21, 0, 0), "Validierung von Patient und Einsendung (Meningokokken)", ChangeType.Feature),
                new Change(new DateTime(2019, 01, 16, 18, 0, 0), "Überarbeitung von Patient und Einsendung (Meningokokken)", ChangeType.Feature),
                new Change(new DateTime(2019, 01, 14), "Invasive und nicht-invasive Entnahmeorte (Meningokokken)", ChangeType.Feature),
                new Change(new DateTime(2019, 01, 13), "Integration der PubMLST Isolat API (Meningokokken)", ChangeType.Feature),
                new Change(new DateTime(2019, 01, 12), "Eingabemaske für Isolate ohne E-Test und ST (Meningokokken)", ChangeType.Feature),
                new Change(new DateTime(2019, 01, 08), "Entfernung der fehlerhaften Warnung bei Teilbefunden", ChangeType.Bug),
                new Change(new DateTime(2019, 01, 07), "Update der Postleitzahl Abfrage", ChangeType.Feature),
                new Change(new DateTime(2019, 01, 06), "Erster Entwurf der Eingabemaske für Isolate (Meningokokken).", ChangeType.Feature),
                new Change(new DateTime(2018, 12, 16), "Korrektur der Interpretationssätze für Endbefunde.", ChangeType.Bug),
                new Change(new DateTime(2018, 12, 15), "Feste Skala für E-Test Werte", ChangeType.Feature),
                new Change(new DateTime(2018, 7, 25), "Anpassung des Sicherheitsprotokolles zur Kommunikation mit dem RKI Tool", ChangeType.Bug),
                new Change(new DateTime(2018, 7, 22), "Update der Interpretationssätze für End- und Teilbefunde.", ChangeType.Feature),
                new Change(new DateTime(2018, 6, 30), "<p>Einführung von Teilbefunden</p>" +
                                                      "<p>Es gibt jetzt eine Befundvorlage mit dem Präfix \"Teilbefund -\" welche zur verwendet " +
                                                      "werden kann um einem Einsender zeitnah den Serotyp mitzuteilen.</p>" +
                                                      "<p>Einsendungen mit Teilbefunden werden in der Übersicht mit einem speziellen Symbol dargestellt:</p>" +
                                                      "<img class='img-rounded img-responsive' src='~/Images/changes/20180703.png'/>" +
                                                      "<p>Sobald einmal ein Endbefund erstellt wurde lässt sich der Zustand einer nicht mehr auf \"Teilbefund\" zurücksetzen.", ChangeType.Feature),
                new Change(new DateTime(2018, 6, 26), "Bereinigung von Fehlerhaften Telefonnummern vom RKI Tool.", ChangeType.Bug),
                new Change(new DateTime(2018, 6, 18, 22, 0, 0), "Kombinierte Eingabemaske für Patent und Einsendungen (Meningokokken).", ChangeType.Feature),
                new Change(new DateTime(2018, 6, 16, 22, 0, 0), "Eingabemaske für Einsendungen (Meningokokken).", ChangeType.Feature),
                new Change(new DateTime(2018, 6, 14, 22, 0, 0), "Eingabemaske für Patienten (Meningokokken).", ChangeType.Feature),
                new Change(new DateTime(2018, 3, 18), "Überarbeitung der RKI Match Funktion (Duplikate, Matching mit Labornummer).", ChangeType.Feature),
                new Change(new DateTime(2018, 2, 21), "Beim ersten Anlegen einer Einsendung werden die klinischen Angaben nicht gespeichert.", ChangeType.Bug),
                new Change(new DateTime(2018, 2, 20), "Import-Funktion für RKI Match Datensätze", ChangeType.Feature),
                new Change(new DateTime(2017, 7, 3, 21, 0, 0), "Bei mehreren gefundenen Gesundheitsämtern wird das Erste zurückgeliefert", ChangeType.Feature),
                new Change(new DateTime(2017, 7, 3, 20, 0, 0), "Bearbeiten von Gesundheitsämtern mit Umlaut schlägt fehl", ChangeType.Bug),
                new Change(new DateTime(2017, 6, 16), "Korrektur der geänderten Schnittstelle zum RKITool", ChangeType.Bug),
                new Change(new DateTime(2017, 5, 31, 20, 0, 0),
                    "<p>Verbesserung der Zuordnung von Gesundheitsamtdaten</p>" +
                    "<p>Die vom RKI abgerufenen Datensätze werden nun mittels Adresse zugeordnet. D.h. wenn lokal ein veränderter Datensatz " +
                    "mit der gleichen Postadresse existiert, dann wird dieser zurückgegeben. Somit ist es möglich die Telefonnummer, Faxnummer " +
                    "und Email-Adresse zu ändern. Diese Änderung gilt dann automatisch für alle Postleitzahlen im Einzugsbereich dieses Gesundheitsamtes.</p>  " , ChangeType.Feature),
                new Change(new DateTime(2017, 5, 31, 18, 0, 0), "Anpassung der Gesundheitsamtabfrage an Änderung des RKI", ChangeType.Feature),
                new Change(new DateTime(2017, 4, 21), "Drittes Zusatzfeld für 'ftsI'", ChangeType.Feature),
                new Change(new DateTime(2017, 4, 20),
                    "<p>Bearbeitung von Gesundheitsamtsdaten</p>" +
                    "<p>Die RKI Datensätze zu den zuständigen Gesundheitsämtern sind oft nicht aktuell. " +
                    "Daher ist es jetzt möglich ein vom RKI-Tool ermitteltes Amt nachträglich zu editieren.</p>" +
                    "<p>Hierfür gibt es zum einen in der Befund-Maske einen entsprechende Aktion:</p>" +
                    "<img class='img-rounded img-responsive' src='~/Images/changes/20170420_1.png'/>" +
                    "<p>Zudem gibt es auch im Administrations-Menü einen Eintrag für Gesundheitsämter:</p>" +
                    "<img class='img-rounded img-responsive' src='~/Images/changes/20170420_2.png'/>" +
                    "<p>Unbekannte Postleitzahlen werden mittels des RKI-Tools nachgeschlagen und anschließend " +
                    "in der Datenbank gespeichert. Dort lassen sie sich dann wie andere Datensätze editieren.<p>" +
                    "<p>Weiterhin ist es möglich zu bestimmten Postleitzahlen eigene Datensätze anzulegen, falls " +
                    "über das RKI-Tool keine Treffer gefunden werden.</p>", ChangeType.Feature),
                new Change(new DateTime(2017, 1, 31), "Hinzufügen der EUCAST Grenzwerte für 2017", ChangeType.Feature),
                new Change(new DateTime(2017, 1, 30), "'Typisches Wachstum auf KB' ist nicht korrekt vorausgewählt.", ChangeType.Bug),
                new Change(new DateTime(2016, 5, 13),
                    "<p>Allgemeine Implementierung für E-Test Messwerte</p>" +
                    "<p>Felder für neue Antibiotika Messwerte lassen sich jetzt im Menü 'Administration->EUCAST Grenzwerte' definieren. " +
                    "Es ist dabei möglich Antibiotika mit und ohne EUCAST-Daten einzutragen. Falls keine EUCAST Grenzwerte für ein " +
                    "Antibiotikum definiert sind wird die Resistenz-Einstufung immer als 'n.d.' gewertet.</p>" +
                    "<p>In der 'Isolat bearbeiten' Maske gibt es weiter feste Felder für die gängigen Antibiotika. Es ist aber auch möglich " +
                    "ein weniger häufig getestetes Medikament via Auswahlfeld zu wählen</p>" +
                    "<img class='img-rounded img-responsive' src='~/Images/changes/20160513_1.png'/>" +
                    "<p>und einen Messwert dafür einzutragen</p>" +
                    "<img class='img-rounded img-responsive' src='~/Images/changes/20160513_2.png'/>" +
                    "<p>Beim Speichern des Isolates wird der Messwert entsprechend hinzugefügt. Um ein weiteres 'Nicht-Standard' " +
                    "Antibiotikum hinzuzufügen muss die Bearbeitungs-Maske nach dem Speichern erneut geöffnet werden.</p>" +
                    "<p>Die Auswahlliste enthält nur die Antibiotika, die im administrativen Bereich angelegt wurden.</p>", ChangeType.Feature),
                new Change(new DateTime(2016, 5, 4), "Umbenennen und umsortieren der Spaltennamen für den RKI Export", ChangeType.Feature),
                new Change(new DateTime(2016, 4, 30), "Spezieller Zugang für das Robert Koch Institut", ChangeType.Feature),
                new Change(new DateTime(2016, 3, 22), "Neue Antibiotikafelder für Imipenem  und Ciprofloxacin.", ChangeType.Feature),
                new Change(new DateTime(2016, 2, 4), "Unterstützung von ausländischen Postleitzahlen.", ChangeType.Feature),
                new Change(new DateTime(2016, 2, 4),
                    "<p>Warnung bei ungespeicherten Änderungen</p>" +
                    "<p>Beim Versuch eine Seite mit einem Formular zu verlassen, welches " +
                    "ungespeicherte Änderungen enthält, wird ein Warndialog angezeigt.</p>", ChangeType.Feature),
                new Change(new DateTime(2016, 2, 4),
                    "<p>Fehlende Information bei Abfrage Gesundheitsamt</p>" +
                    "<p>Wenn der Datensatz vom RKI-Tool keine Telefonnummer enthält wird " +
                    "die ggf. vorhandene Faxnummer und Email-Adresse nicht angezeigt</p>", ChangeType.Bug),
                new Change(new DateTime(2016, 2, 3),
                    "<p>Aktualisierung der Befundvorlagen</p>" +
                    "<ul>" +
                    "<li>Korrektur der Grußformel</li>" +
                    "<li>Neue Vorlage: Ampi-S Cipro-R</li>" +
                    "<li>Neue Vorlage: Ampi-S Imi-R</li>" +
                    "<li>Neue Vorlage: Kein Haemophilus v1</li>" +
                    "<li>Neue Vorlage: lowBLNAR CTX-R Imi-R</li>" +
                    "</ul>",
                    ChangeType.Feature),
                new Change(new DateTime(2016, 2, 3), "Hinzufügen der EUCAST Grenzwerte für 2016", ChangeType.Feature),
                new Change(new DateTime(2016, 1, 3),
                    "<p>Exort der Einsenderdaten.</p>" +
                    "<p>Unter dem Menüpunkt \"Export->Einsender Export\" gibt es jezt eine Maske um alle Einsender aus einem bestimmten Zeitraum zu exportieren.</p>" +
                    "<p>Der Excel-Export enthält alle Einsender von denen Einsendungen mit einem Entnahme-/Eingangsdatum im gewählten Zeitraum existieren.</p>",
                    ChangeType.Feature),
                new Change(new DateTime(2015, 11, 16),
                    "<p>Löschen von Einsendern ist jetzt trotz zugeordneter Einsendungen möglich.</p>" +
                    "<p>Gelöschte Einsendern sind jedoch nur noch in der Bearbeitungsmaske von zughörigen Einsendungen sichtbar.</p>",
                    ChangeType.Feature),
                new Change(new DateTime(2015, 10, 31), "<p>Automatische Bestimmung des zugehörigen Gesundheitsamtes</p>" +
                                                       "<p>Anhand der Postleitzahl des Patienten wird über eine Hintergrundabfrage an das <a href='https://tools.rki.de/plztool/'>RKI PLZ Tool</a> " +
                                                       "ermittelt, welches Gesundheitsamt zuständig ist.",
                    ChangeType.Feature),
                new Change(new DateTime(2015, 10, 21), "<p>Befundvorlagen für Fax-Versand</p>" +
                                                       "<p>In der Liste der Befundvorlagen sind gibt es jetzt Einträge mit dem Prefix \"Fax\".</p>" +
                                                       "<p>Die Fax-Vorlagen beinhalten neben der Empfangsbestätigung noch eine Zeile die darauf " +
                                                       "hinweist, dass das Dokument ohne Unterschrift gültig ist.</p>",
                    ChangeType.Feature),
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