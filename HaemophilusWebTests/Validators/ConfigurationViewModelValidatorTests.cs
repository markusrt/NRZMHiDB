using System;
using System.Collections.Generic;
using FluentAssertions;
using HaemophilusWeb.ViewModels;
using NUnit.Framework;

namespace HaemophilusWeb.Validators
{
    [TestFixture]
    public class ConfigurationViewModelValidatorTests
        : AbstractValidatorTests<ConfigurationViewModelValidator, ConfigurationViewModel>
    {
        protected static readonly IEnumerable<Tuple<ConfigurationViewModel, string[]>> InvalidModels;

        static ConfigurationViewModelValidatorTests()
        {
            InvalidModels = CreateInvalidModels();
        }

        protected override ConfigurationViewModel CreateValidModel()
        {
            return new ConfigurationViewModel
            {
                ReportSigners = "Dr. A\nDr. B",
                AnnouncementText = "Hinweis",
                AnnouncementStart = new DateTime(2026, 1, 1),
                AnnouncementEnd = new DateTime(2026, 1, 31),
                LabDirector = "Dr. Lab",
                MedicalDirector = "Dr. Med",
                Contacts = "Dr. A\nTelefon: 1\na@b.de"
            };
        }

        [Test]
        public void EmptyAnnouncement_WithoutDates_IsValid()
        {
            var model = new ConfigurationViewModel { ReportSigners = "Dr. A" };

            Validate(model).IsValid.Should().BeTrue();
        }

        private static IEnumerable<Tuple<ConfigurationViewModel, string[]>> CreateInvalidModels()
        {
            // Announcement text set, but no validity range
            yield return Tuple.Create(new ConfigurationViewModel
            {
                AnnouncementText = "Hinweis"
            }, new[] { "AnnouncementStart", "AnnouncementEnd" });

            // End before start (with text)
            yield return Tuple.Create(new ConfigurationViewModel
            {
                AnnouncementText = "Hinweis",
                AnnouncementStart = new DateTime(2026, 1, 10),
                AnnouncementEnd = new DateTime(2026, 1, 9)
            }, new[] { "AnnouncementEnd" });

            // End before start (without text – range rule still fires)
            yield return Tuple.Create(new ConfigurationViewModel
            {
                AnnouncementStart = new DateTime(2026, 1, 10),
                AnnouncementEnd = new DateTime(2026, 1, 9)
            }, new[] { "AnnouncementEnd" });
        }

        [Test]
        public void SameDayRange_IsValid()
        {
            var model = new ConfigurationViewModel
            {
                AnnouncementText = "Hinweis",
                AnnouncementStart = new DateTime(2026, 1, 10),
                AnnouncementEnd = new DateTime(2026, 1, 10)
            };

            Validate(model).IsValid.Should().BeTrue();
        }
    }
}
