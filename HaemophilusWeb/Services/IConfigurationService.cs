using System.Collections.Generic;

namespace HaemophilusWeb.Services
{
    public interface IConfigurationService
    {
        T Get<T>(string key, T fallback = default(T));

        void Set<T>(string key, T value);

        List<string> GetReportSigners();

        void SetReportSigners(IEnumerable<string> signers);

        AnnouncementSetting GetAnnouncement();

        void SetAnnouncement(AnnouncementSetting announcement);

        string GetActiveAnnouncement();

        string GetLabDirector();

        void SetLabDirector(string name);

        string GetMedicalDirector();

        void SetMedicalDirector(string name);

        List<string> GetContacts();

        void SetContacts(IEnumerable<string> contacts);
    }
}
