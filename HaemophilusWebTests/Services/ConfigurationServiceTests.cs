using System;
using System.Linq;
using FluentAssertions;
using HaemophilusWeb.Models;
using NUnit.Framework;

namespace HaemophilusWeb.Services
{
    [TestFixture]
    public class ConfigurationServiceTests
    {
        private ApplicationDbContextMock db;
        private ConfigurationService sut;

        [SetUp]
        public void SetUp()
        {
            db = new ApplicationDbContextMock();
            sut = new ConfigurationService(db);
        }

        [Test]
        public void GetReportSigners_NoEntry_ReturnsEmptyList()
        {
            sut.GetReportSigners().Should().BeEmpty();
        }

        [Test]
        public void GetLabDirector_NoEntry_ReturnsEmptyString()
        {
            sut.GetLabDirector().Should().BeEmpty();
        }

        [Test]
        public void GetContacts_NoEntry_ReturnsEmptyList()
        {
            sut.GetContacts().Should().BeEmpty();
        }

        [Test]
        public void Get_MissingKey_ReturnsFallback()
        {
            sut.Get("does-not-exist", "fallback").Should().Be("fallback");
        }

        [Test]
        public void SetAndGetReportSigners_RoundTrips()
        {
            sut.SetReportSigners(new[] { "Dr. A", "Dr. B" });

            sut.GetReportSigners().Should().Equal("Dr. A", "Dr. B");
        }

        [Test]
        public void SetReportSigners_TrimsAndDropsBlankEntries()
        {
            sut.SetReportSigners(new[] { "  Dr. A  ", "", "   ", "Dr. B" });

            sut.GetReportSigners().Should().Equal("Dr. A", "Dr. B");
        }

        [Test]
        public void SetContacts_RoundTripsMultilineBlocks()
        {
            sut.SetContacts(new[] { "Dr. A\nTelefon: 1", "Dr. B\nTelefon: 2" });

            sut.GetContacts().Should().Equal("Dr. A\nTelefon: 1", "Dr. B\nTelefon: 2");
        }

        [Test]
        public void Set_SameKeyTwice_DoesNotCreateDuplicateEntries()
        {
            sut.SetLabDirector("First");
            sut.SetLabDirector("Second");

            sut.GetLabDirector().Should().Be("Second");
            db.ConfigurationEntriesDbSet.Count(e => e.Key == ConfigurationService.Keys.LabDirector).Should().Be(1);
        }

        [Test]
        public void Announcement_RoundTrips()
        {
            sut.SetAnnouncement(new AnnouncementSetting
            {
                Text = "Hi",
                Start = new DateTime(2026, 1, 1),
                End = new DateTime(2026, 1, 2)
            });

            var loaded = sut.GetAnnouncement();
            loaded.Text.Should().Be("Hi");
            loaded.Start.Should().Be(new DateTime(2026, 1, 1));
            loaded.End.Should().Be(new DateTime(2026, 1, 2));
        }

        [Test]
        public void GetActiveAnnouncement_TodayWithinRange_ReturnsText()
        {
            sut.SetAnnouncement(new AnnouncementSetting
            {
                Text = "Aktiv",
                Start = DateTime.Today.AddDays(-1),
                End = DateTime.Today.AddDays(1)
            });

            sut.GetActiveAnnouncement().Should().Be("Aktiv");
        }

        [Test]
        public void GetActiveAnnouncement_BeforeRange_ReturnsEmpty()
        {
            sut.SetAnnouncement(new AnnouncementSetting
            {
                Text = "Zukunft",
                Start = DateTime.Today.AddDays(1),
                End = DateTime.Today.AddDays(5)
            });

            sut.GetActiveAnnouncement().Should().BeEmpty();
        }

        [Test]
        public void GetActiveAnnouncement_AfterRange_ReturnsEmpty()
        {
            sut.SetAnnouncement(new AnnouncementSetting
            {
                Text = "Vergangen",
                Start = DateTime.Today.AddDays(-5),
                End = DateTime.Today.AddDays(-1)
            });

            sut.GetActiveAnnouncement().Should().BeEmpty();
        }

        [Test]
        public void GetActiveAnnouncement_EmptyText_ReturnsEmpty()
        {
            sut.SetAnnouncement(new AnnouncementSetting
            {
                Text = "",
                Start = DateTime.Today.AddDays(-1),
                End = DateTime.Today.AddDays(1)
            });

            sut.GetActiveAnnouncement().Should().BeEmpty();
        }

        // specs/018 FR-013, SC-003: the first and the last day of the period are both inclusive
        [TestCase(1, 1, 10, true)]
        [TestCase(10, 1, 10, true)]
        [TestCase(5, 1, 10, true)]
        [TestCase(11, 1, 10, false)]
        [TestCase(4, 5, 10, false)]
        public void AnnouncementIsActiveOn_PeriodBoundariesAreInclusive(int day, int startDay, int endDay, bool expected)
        {
            var announcement = new AnnouncementSetting
            {
                Text = "Hinweis",
                Start = new DateTime(2026, 11, startDay),
                End = new DateTime(2026, 11, endDay, 0, 0, 0)
            };

            announcement.IsActiveOn(new DateTime(2026, 11, day, 23, 59, 0)).Should().Be(expected);
        }

        // specs/018 Edge Cases "Announcement period is always closed in the page": a stored announcement without
        // dates is treated as always active
        [Test]
        public void AnnouncementIsActiveOn_WithoutDates_IsActive()
        {
            new AnnouncementSetting { Text = "Hinweis" }.IsActiveOn(new DateTime(2026, 11, 8)).Should().BeTrue();
        }

        // specs/018 Edge Cases "Unreadable stored value": a value that cannot be read is treated as empty
        [Test]
        public void Get_UnreadableStoredValue_ReturnsEmptySettings()
        {
            foreach (var key in new[]
                     {
                         ConfigurationService.Keys.ReportSigners, ConfigurationService.Keys.Announcement,
                         ConfigurationService.Keys.LabDirector, ConfigurationService.Keys.MedicalDirector,
                         ConfigurationService.Keys.Contacts
                     })
            {
                db.ConfigurationEntriesDbSet.Add(new ConfigurationEntry { Key = key, Value = "{not json" });
            }

            sut.GetReportSigners().Should().BeEmpty();
            sut.GetAnnouncement().Text.Should().BeNull();
            sut.GetActiveAnnouncement().Should().BeEmpty();
            sut.GetLabDirector().Should().BeEmpty();
            sut.GetMedicalDirector().Should().BeEmpty();
            sut.GetContacts().Should().BeEmpty();
        }
    }
}
