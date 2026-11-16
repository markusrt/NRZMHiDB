using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using FluentAssertions;
using HaemophilusWeb.Mocks;
using HaemophilusWeb.Models;
using HaemophilusWeb.Services;
using HaemophilusWeb.ViewModels;
using Newtonsoft.Json;
using NSubstitute;
using NUnit.Framework;

namespace HaemophilusWeb.Controllers
{
    // specs/018: configuration page (Konfiguration); added in the 018 verification, closes the "configuration page"
    // part of TEST-001
    [TestFixture]
    public class ConfigurationControllerTests
    {
        private HashSet<ConfigurationEntry> entries;
        private IApplicationDbContext db;
        private ConfigurationController controller;

        [SetUp]
        public void SetUp()
        {
            entries = new HashSet<ConfigurationEntry>();
            db = Substitute.For<IApplicationDbContext>();
            db.ConfigurationEntries.Returns(new InMemoryDbSet<ConfigurationEntry>(entries));
            controller = new ConfigurationController(db);
        }

        // specs/018 US4.5: the address of the configuration area without a page name leads to the page
        [Test]
        public void Index_RedirectsToEdit()
        {
            var result = controller.Index() as RedirectToRouteResult;

            result.Should().NotBeNull();
            result.RouteValues["action"].Should().Be("Edit");
        }

        // specs/018 FR-003, US1.2, US3.3: the page shows the stored values, signers one per line, contact blocks
        // separated by one empty line
        [Test]
        public void EditGet_ShowsStoredValues()
        {
            Store(ConfigurationService.Keys.ReportSigners, new[] { "Dr. A", "Dr. B" });
            Store(ConfigurationService.Keys.Announcement, new AnnouncementSetting
            {
                Text = "Hinweis", Start = new DateTime(2026, 11, 1), End = new DateTime(2026, 11, 30)
            });
            Store(ConfigurationService.Keys.LabDirector, "Lab");
            Store(ConfigurationService.Keys.MedicalDirector, "Med");
            Store(ConfigurationService.Keys.Contacts, new[] { "A\nTel 1", "B\nTel 2" });

            var model = (ConfigurationViewModel) ((ViewResult) controller.Edit()).Model;

            model.ReportSigners.Should().Be("Dr. A" + Environment.NewLine + "Dr. B");
            model.AnnouncementText.Should().Be("Hinweis");
            model.AnnouncementStart.Should().Be(new DateTime(2026, 11, 1));
            model.AnnouncementEnd.Should().Be(new DateTime(2026, 11, 30));
            model.LabDirector.Should().Be("Lab");
            model.MedicalDirector.Should().Be("Med");
            model.Contacts.Should().Be("A\nTel 1" + Environment.NewLine + Environment.NewLine + "B\nTel 2");
        }

        // specs/018 FR-004, US1.3, US1.6: one signer per non-blank line, trimmed, in entered order, duplicates kept
        [Test]
        public void EditPost_StoresSignersPerNonBlankLineTrimmedInOrderWithDuplicates()
        {
            controller.Edit(new ConfigurationViewModel { ReportSigners = "  Dr. B  \r\n\r\n   \r\nDr. A\nDr. B\r\n" });

            Read<List<string>>(ConfigurationService.Keys.ReportSigners).Should().Equal("Dr. B", "Dr. A", "Dr. B");
        }

        // specs/018 FR-005, US3.3: blocks split at empty or blank lines, trimmed at their ends, inner line breaks
        // kept, empty blocks dropped, order kept
        [Test]
        public void EditPost_StoresContactBlocksSplitAtBlankLines()
        {
            controller.Edit(new ConfigurationViewModel
            {
                Contacts = "\r\n  Kontakt A  \r\nTelefon 1  \r\n   \r\nKontakt B\r\nZeile 2\r\n\r\n\r\n\r\nKontakt C\r\n\r\n"
            });

            Read<List<string>>(ConfigurationService.Keys.Contacts)
                .Should().Equal("Kontakt A  \nTelefon 1", "Kontakt B\nZeile 2", "Kontakt C");
        }

        // specs/018 FR-006, US3.2: director names trimmed, empty values accepted
        [Test]
        public void EditPost_StoresDirectorNamesTrimmedAndAcceptsEmpty()
        {
            controller.Edit(new ConfigurationViewModel { LabDirector = "  Lab  ", MedicalDirector = null });

            Read<string>(ConfigurationService.Keys.LabDirector).Should().Be("Lab");
            Read<string>(ConfigurationService.Keys.MedicalDirector).Should().BeEmpty();
        }

        // specs/018 FR-007, US2.4: announcement text trimmed with unified line breaks, stored with both dates
        [Test]
        public void EditPost_StoresAnnouncementTrimmedWithUnifiedLineBreaksAndDates()
        {
            controller.Edit(new ConfigurationViewModel
            {
                AnnouncementText = "  \r\n Zeile 1\r\nZeile 2\rZeile 3  \r\n",
                AnnouncementStart = new DateTime(2026, 11, 7),
                AnnouncementEnd = new DateTime(2026, 11, 9)
            });

            var stored = Read<AnnouncementSetting>(ConfigurationService.Keys.Announcement);
            stored.Text.Should().Be("Zeile 1\nZeile 2\nZeile 3");
            stored.Start.Should().Be(new DateTime(2026, 11, 7));
            stored.End.Should().Be(new DateTime(2026, 11, 9));
        }

        // specs/018 Edge Cases "Dates without text": dates are stored without a text
        [Test]
        public void EditPost_StoresDatesWithoutText()
        {
            controller.Edit(new ConfigurationViewModel
            {
                AnnouncementText = "   ",
                AnnouncementStart = new DateTime(2026, 11, 7),
                AnnouncementEnd = new DateTime(2026, 11, 9)
            });

            var stored = Read<AnnouncementSetting>(ConfigurationService.Keys.Announcement);
            stored.Text.Should().BeEmpty();
            stored.Start.Should().Be(new DateTime(2026, 11, 7));
            stored.IsActiveOn(new DateTime(2026, 11, 8)).Should().BeFalse();
        }

        // specs/018 FR-008, US2.2, SC-002: on a validation failure the page is shown again and nothing is stored
        [Test]
        public void EditPost_InvalidModel_ShowsPageAgainAndStoresNothing()
        {
            var model = new ConfigurationViewModel { ReportSigners = "Dr. X", AnnouncementText = "Hinweis" };
            controller.ModelState.AddModelError("AnnouncementStart", "Bitte ein Startdatum für die Mitteilung angeben.");

            var result = controller.Edit(model) as ViewResult;

            result.Should().NotBeNull();
            result.Model.Should().BeSameAs(model);
            entries.Should().BeEmpty();
            db.DidNotReceive().SaveChanges();
        }

        // specs/018 FR-009, US1.3: a successful save stores all five settings, sets the message and shows the page again
        [Test]
        public void EditPost_Success_StoresFiveSettingsAndRedirectsWithMessage()
        {
            var result = controller.Edit(new ConfigurationViewModel { ReportSigners = "Dr. A" }) as RedirectToRouteResult;

            result.Should().NotBeNull();
            result.RouteValues["action"].Should().Be("Edit");
            controller.TempData["SuccessMessage"].Should().Be("Konfiguration gespeichert.");
            entries.Select(e => e.Key).Should().BeEquivalentTo(
                ConfigurationService.Keys.ReportSigners, ConfigurationService.Keys.Announcement,
                ConfigurationService.Keys.LabDirector, ConfigurationService.Keys.MedicalDirector,
                ConfigurationService.Keys.Contacts);
        }

        // specs/018 FR-010: the save verifies the anti-forgery token
        [Test]
        public void EditPost_RequiresAntiForgeryToken()
        {
            var method = typeof(ConfigurationController).GetMethod("Edit", new[] { typeof(ConfigurationViewModel) });

            method.GetCustomAttributes(typeof(ValidateAntiForgeryTokenAttribute), false).Should().HaveCount(1);
            method.GetCustomAttributes(typeof(HttpPostAttribute), false).Should().HaveCount(1);
        }

        private void Store<T>(string key, T value)
        {
            entries.Add(new ConfigurationEntry { Key = key, Value = JsonConvert.SerializeObject(value) });
        }

        private T Read<T>(string key)
        {
            return JsonConvert.DeserializeObject<T>(entries.Single(e => e.Key == key).Value);
        }
    }
}
