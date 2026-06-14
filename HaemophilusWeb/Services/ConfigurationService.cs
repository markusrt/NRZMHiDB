using System;
using System.Collections.Generic;
using System.Linq;
using HaemophilusWeb.Models;
using Newtonsoft.Json;

namespace HaemophilusWeb.Services
{
    public class ConfigurationService : IConfigurationService
    {
        public static class Keys
        {
            public const string ReportSigners = "ReportSigners";
            public const string Announcement = "Announcement";
            public const string LabDirector = "LabDirector";
            public const string MedicalDirector = "MedicalDirector";
            public const string Contacts = "Contacts";
        }

        private readonly IApplicationDbContext db;

        public ConfigurationService(IApplicationDbContext db)
        {
            this.db = db;
        }

        public T Get<T>(string key, T fallback = default(T))
        {
            var entry = db.ConfigurationEntries.FirstOrDefault(e => e.Key == key);
            if (entry == null || string.IsNullOrEmpty(entry.Value))
            {
                return fallback;
            }

            try
            {
                return JsonConvert.DeserializeObject<T>(entry.Value);
            }
            catch (JsonException)
            {
                return fallback;
            }
        }

        public void Set<T>(string key, T value)
        {
            var json = JsonConvert.SerializeObject(value);
            var entry = db.ConfigurationEntries.FirstOrDefault(e => e.Key == key);
            if (entry == null)
            {
                db.ConfigurationEntries.Add(new ConfigurationEntry { Key = key, Value = json });
            }
            else
            {
                entry.Value = json;
                db.MarkAsModified(entry);
            }

            db.SaveChanges();
        }

        public List<string> GetReportSigners()
        {
            return Get(Keys.ReportSigners, new List<string>()) ?? new List<string>();
        }

        public void SetReportSigners(IEnumerable<string> signers)
        {
            Set(Keys.ReportSigners, Sanitize(signers));
        }

        public AnnouncementSetting GetAnnouncement()
        {
            return Get(Keys.Announcement, new AnnouncementSetting()) ?? new AnnouncementSetting();
        }

        public void SetAnnouncement(AnnouncementSetting announcement)
        {
            Set(Keys.Announcement, announcement ?? new AnnouncementSetting());
        }

        public string GetActiveAnnouncement()
        {
            var announcement = GetAnnouncement();
            return announcement.IsActiveOn(DateTime.Today) ? announcement.Text : string.Empty;
        }

        public string GetLabDirector()
        {
            return Get(Keys.LabDirector, string.Empty) ?? string.Empty;
        }

        public void SetLabDirector(string name)
        {
            Set(Keys.LabDirector, name ?? string.Empty);
        }

        public string GetMedicalDirector()
        {
            return Get(Keys.MedicalDirector, string.Empty) ?? string.Empty;
        }

        public void SetMedicalDirector(string name)
        {
            Set(Keys.MedicalDirector, name ?? string.Empty);
        }

        public List<string> GetContacts()
        {
            return Get(Keys.Contacts, new List<string>()) ?? new List<string>();
        }

        public void SetContacts(IEnumerable<string> contacts)
        {
            Set(Keys.Contacts, Sanitize(contacts));
        }

        private static List<string> Sanitize(IEnumerable<string> values)
        {
            return (values ?? Enumerable.Empty<string>())
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Select(value => value.Trim())
                .ToList();
        }
    }
}
