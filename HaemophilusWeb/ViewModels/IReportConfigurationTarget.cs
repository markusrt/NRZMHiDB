namespace HaemophilusWeb.ViewModels
{
    public interface IReportConfigurationTarget
    {
        string Announcement { get; set; }

        string LabDirector { get; set; }

        string MedicalDirector { get; set; }

        string Contacts { get; set; }
    }
}
