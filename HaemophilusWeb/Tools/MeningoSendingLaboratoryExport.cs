using HaemophilusWeb.Domain;
using HaemophilusWeb.Models;
using HaemophilusWeb.Models.Meningo;
using HaemophilusWeb.Utils;

namespace HaemophilusWeb.Tools
{
    public class MeningoSendingLaboratoryExport : SendingExportDefinition<MeningoSending, MeningoPatient>
    {
        public MeningoSendingLaboratoryExport()
        {
            AddField(s => s.Isolate.LaboratoryNumberWithPrefix);
            AddField(s => s.Isolate.StemNumberWithPrefix);
            AddField(s => s.SenderId);
            AddField(s => s.ReceivingDate.ToReportFormat());
            AddField(s => s.SamplingDate.ToReportFormat());
            AddField(s => s.SenderLaboratoryNumber);
            AddField(s => ExportSamplingLocation(s.SamplingLocation, s));
            AddField(s => ExportToString(s.Material));
            AddField(s => ExportToString(s.Invasive));
            AddField(s => s.SerogroupSender);

            AddField(s => s.Patient.Initials);
            AddField(s => s.Patient.BirthDate.ToReportFormat());
            AddField(s => s.Isolate.PatientAgeAtSampling(), "Patientenalter bei Entnahme");
            AddField(s => ExportToString(s.Patient.Gender));
            AddField(s => s.Patient.PostalCode);
            AddField(s => s.Patient.City);
            AddField(s => ExportToString(s.Patient.County));
            AddField(s => ExportToString(s.Patient.State));
            AddField(s => ExportClinicalInformation(s.Patient.ClinicalInformation, () => s.Patient.OtherClinicalInformation, _ => _.HasFlag(MeningoClinicalInformation.Other)));
            AddField(s => s.Remark, "Bemerkung (Einsendung)");

            AddField(s => ExportToString(s.Isolate.GrowthOnBloodAgar));
            AddField(s => ExportToString(s.Isolate.GrowthOnMartinLewisAgar));
            AddField(s => ExportToString(s.Isolate.Oxidase));
            AddField(s => ExportToString(s.Isolate.Agglutination));
            AddField(s => ExportToString(s.Isolate.SerogroupPcr));

            AddEpsilometerTestFields(this, Antibiotic.Benzylpenicillin);
            AddEpsilometerTestFields(this, Antibiotic.Ciprofloxacin);
            AddEpsilometerTestFields(this, Antibiotic.Rifampicin);
            AddEpsilometerTestFields(this, Antibiotic.Cefotaxime);
            AddField(s => ExportToString(s.Isolate.RibosomalRna16S));
            AddField(s => ExportToString(s.Isolate.RibosomalRna16SBestMatch));
            AddField(s => ExportToString(s.Isolate.RibosomalRna16SMatchInPercent));
            AddField(s => ExportToString(s.Isolate.MaldiTof));
            AddField(s => s.Isolate.MaldiTofBestMatch);
            AddField(s => s.Isolate.MaldiTofMatchConfidence);
            AddField(s => s.Isolate.ReportDate);
            AddField(s => s.Isolate.Remark, "Bemerkung (Isolat)");

            AddField(s => ExportChildProperty(
                s.Isolate.NeisseriaPubMlstIsolate, _ => _.PubMlstId, "-"), "PubMLST ID");
        }



        private static string ExportSamplingLocation(MeningoSamplingLocation samplingLocation, MeningoSending sending)
        {
            return samplingLocation == MeningoSamplingLocation.OtherInvasive
                ? sending.OtherInvasiveSamplingLocation
                : samplingLocation == MeningoSamplingLocation.OtherNonInvasive
                    ? sending.OtherNonInvasiveSamplingLocation
                    : ExportToString(samplingLocation);
        }
    }
}