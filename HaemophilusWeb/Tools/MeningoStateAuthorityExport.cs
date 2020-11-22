using System.Linq;
using HaemophilusWeb.Models.Meningo;
using HaemophilusWeb.Utils;

namespace HaemophilusWeb.Tools
{
    public class MeningoStateAuthorityExport : SendingExportDefinition<MeningoSending, MeningoPatient>
    {
        public MeningoStateAuthorityExport()
        {
            AddField(s => s.MeningoPatientId, "Patientnr NRZM");
            AddField(s => s.Patient.Gender.HasValue ? ExportToString(s.Patient.Gender).Substring(0,1) : null, "Geschlecht");
            AddField(s => s.Patient.BirthDate.ToReportFormatMonthYear(), "Geburtsmonat");
            AddField(s => s.ReceivingDate.ToReportFormat());
            AddField(s => s.SamplingDate.ToReportFormat());
            AddField(s => ExportToString(s.Isolate.Agglutination), "Serogruppe");
            AddField(s => ExportToString(s.Isolate.PorAVr1), "PorA VR1");
            AddField(s => ExportToString(s.Isolate.PorAVr2), "PorA VR2");
            AddField(s => ExportToString(s.Isolate.FetAVr), "FetA VR");
            AddField(s => s.Patient.County, "Kreis");
        }
    }
}