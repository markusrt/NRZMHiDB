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
    }
}
