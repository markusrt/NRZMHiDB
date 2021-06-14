using System;
using HaemophilusWeb.Domain;
using HaemophilusWeb.Models;
using HaemophilusWeb.Models.Meningo;
using HaemophilusWeb.Utils;
using HaemophilusWeb.Views.Utils;
using static HaemophilusWeb.Services.PubMlstService;

namespace HaemophilusWeb.Tools
{
    public class MeningoSendingIrisExport : SendingExportDefinition<MeningoSending, MeningoPatient>
    {
        public MeningoSendingIrisExport()
        {
            AddField(s => s.Isolate.LaboratoryNumberWithPrefix);
            AddField(s => s.Isolate.StemNumberWithPrefix);
            AddField(s => ExportToString(s.Patient.State));
            AddField(s => s.ReceivingDate.ToReportFormat());
            AddField(s => s.SamplingDate.ToReportFormat());
            AddField(s => ExportToString(s.Material));
            AddField(s => ExportToString(s.Invasive));
            
            AddField(s => s.Isolate.PatientAgeAtSampling(), "Patientenalter bei Entnahme");
            AddField(s => GetMonthAge(s), "Patientenalter bei Entnahme (Monate)");
            
            AddField(s => ExportToString(s.Patient.Gender));
            AddField(s => ExportClinicalInformation(s.Patient.ClinicalInformation, () => s.Patient.OtherClinicalInformation, _ => _.HasFlag(MeningoClinicalInformation.Other)));
            AddField(s => ExportSamplingLocation(s.SamplingLocation, s));

            AddField(s => ExportToString(s.Isolate.Agglutination));

            AddEpsilometerTestFields(this, Antibiotic.Benzylpenicillin, true);
            AddEpsilometerTestFields(this, Antibiotic.Cefotaxime, true);
            AddEpsilometerTestFields(this, Antibiotic.Rifampicin, true);
            AddEpsilometerTestFields(this, Antibiotic.Ciprofloxacin, true);

            AddFieldOnPositiveTestResult(s => s.Isolate.PorAPcr, s => s.Isolate.PorAVr1, "PorA-VR1");
            AddFieldOnPositiveTestResult(s => s.Isolate.PorAPcr, s => s.Isolate.PorAVr2, "PorA-VR2");
            AddFieldOnPositiveTestResult(s => s.Isolate.FetAPcr, s => s.Isolate.FetAVr, "FetA-VR");

            AddField(s => ExportToString(s.Isolate.SerogroupPcr));
            AddField(s => ExportToString(s.Isolate.RealTimePcr));

            AddField(s => ExportToString(s.Isolate.CsbPcr));
            AddField(s => ExportToString(s.Isolate.CscPcr));
            AddField(s => ExportToString(s.Isolate.CswyPcr));
            AddField(s => ExportToString(s.Isolate.CswyAllele));
        }

        private int? GetMonthAge(MeningoSending sending)
        {
            var monthAge = sending.Isolate.PatientMonthAgeAtSampling();
            return monthAge >=12 ? (int?) null : monthAge;
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