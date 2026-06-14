using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using HaemophilusWeb.Models;
using HaemophilusWeb.Services;
using HaemophilusWeb.ViewModels;

namespace HaemophilusWeb.Controllers
{
    [Authorize(Roles = DefaultRoles.User)]
    public class ConfigurationController : Controller
    {
        private readonly IApplicationDbContext db;
        private readonly IConfigurationService configuration;

        public ConfigurationController()
            : this(new ApplicationDbContextWrapper(new ApplicationDbContext()))
        {
        }

        public ConfigurationController(IApplicationDbContext applicationDbContext)
        {
            db = applicationDbContext;
            configuration = new ConfigurationService(db);
        }

        public ActionResult Index()
        {
            return RedirectToAction("Edit");
        }

        [HttpGet]
        public ActionResult Edit()
        {
            var announcement = configuration.GetAnnouncement();
            var model = new ConfigurationViewModel
            {
                ReportSigners = JoinLines(configuration.GetReportSigners()),
                AnnouncementText = announcement.Text,
                AnnouncementStart = announcement.Start,
                AnnouncementEnd = announcement.End,
                LabDirector = configuration.GetLabDirector(),
                MedicalDirector = configuration.GetMedicalDirector(),
                Contacts = JoinBlocks(configuration.GetContacts())
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ConfigurationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            configuration.SetReportSigners(SplitLines(model.ReportSigners));
            configuration.SetAnnouncement(new AnnouncementSetting
            {
                Text = NormalizeNewlines(model.AnnouncementText).Trim(),
                Start = model.AnnouncementStart,
                End = model.AnnouncementEnd
            });
            configuration.SetLabDirector((model.LabDirector ?? string.Empty).Trim());
            configuration.SetMedicalDirector((model.MedicalDirector ?? string.Empty).Trim());
            configuration.SetContacts(SplitBlocks(model.Contacts));

            TempData["SuccessMessage"] = "Konfiguration gespeichert.";
            return RedirectToAction("Edit");
        }

        private static string NormalizeNewlines(string value)
        {
            return (value ?? string.Empty).Replace("\r\n", "\n").Replace("\r", "\n");
        }

        private static List<string> SplitLines(string value)
        {
            return NormalizeNewlines(value)
                .Split('\n')
                .Select(line => line.Trim())
                .Where(line => line.Length > 0)
                .ToList();
        }

        private static List<string> SplitBlocks(string value)
        {
            return Regex.Split(NormalizeNewlines(value), "\n\\s*\n", RegexOptions.None, TimeSpan.FromMilliseconds(100))
                .Select(block => block.Trim())
                .Where(block => block.Length > 0)
                .ToList();
        }

        private static string JoinLines(IEnumerable<string> values)
        {
            return string.Join(Environment.NewLine, values);
        }

        private static string JoinBlocks(IEnumerable<string> values)
        {
            return string.Join(Environment.NewLine + Environment.NewLine, values);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
