using System;
using HaemophilusWeb.Domain;
using HaemophilusWeb.Models;
using HaemophilusWeb.Models.Meningo;
using HaemophilusWeb.Utils;
using static HaemophilusWeb.Services.PubMlstService;

namespace HaemophilusWeb.Tools
{
    public class HaemophilusSendingLaboratoryExport : SendingExportDefinition<Sending, Patient>
    {
        public HaemophilusSendingLaboratoryExport()
        {
            AddField(s => s.PatientId, "Patienten-Nr.");
            AddField(s => s.Isolate.LaboratoryNumberWithPrefix);
            AddField(s => s.Isolate.StemNumberWithPrefix);
            AddField(s => s.SenderId);
            AddField(s => s.ReceivingDate.ToReportFormat());
            AddField(s => s.SamplingDate.ToReportFormat());
            AddField(s => s.SenderLaboratoryNumber);
            AddField(s => s.DemisId);
            AddField(s => ExportSamplingLocation(s.SamplingLocation, s));
            AddField(s => ExportToString(s.Material));
            AddField(s => ExportToString(s.Invasive));
            AddField(s => s.SenderConclusion);

            AddField(s => s.Patient.Initials);
            AddField(s => s.Patient.BirthDate.ToReportFormat());
            AddField(s => s.Isolate.PatientAgeAtSampling(), "Patientenalter bei Entnahme");
            AddField(s => ExportToString(s.Patient.Gender));
            AddField(s => s.Patient.PostalCode);
            AddField(s => s.Patient.City);
            AddField(s => ExportToString(s.Patient.County));
            AddField(s => ExportToString(s.Patient.State));
            AddField(s => ExportClinicalInformation(
                s.Patient.ClinicalInformation, () => s.Patient.OtherClinicalInformation, _ => _.HasFlag(ClinicalInformation.Other)));
            AddField(s => ExportToString(s.Patient.HibVaccination));
            AddField(s => s.Patient.HibVaccinationDate.ToReportFormat());
            AddField(s => s.Remark, "Bemerkung (Einsendung)");

            AddField(s => ExportToString(s.Isolate.Growth));
            AddField(s => ExportToString(s.Isolate.TypeOfGrowth));
            AddField(s => ExportToString(s.Isolate.Oxidase));
            AddField(s => ExportToString(s.Isolate.BetaLactamase));
            AddField(s => ExportToString(s.Isolate.Agglutination));
            AddField(s => ExportToString(s.Isolate.FactorTest));
            AddEpsilometerTestFields(this, Antibiotic.Ampicillin);
            AddEpsilometerTestFields(this, Antibiotic.AmoxicillinClavulanate);
            AddEpsilometerTestFields(this, Antibiotic.Meropenem);
            AddEpsilometerTestFields(this, Antibiotic.Cefotaxime);
            AddEpsilometerTestFields(this, Antibiotic.Imipenem);
            AddEpsilometerTestFields(this, Antibiotic.Ciprofloxacin);
            AddField(s => ExportToString(s.Isolate.OuterMembraneProteinP2));
            AddField(s => ExportToString(s.Isolate.BexA));
            AddField(s => ExportToString(s.Isolate.SerotypePcr));
            AddField(s => ExportToString(s.Isolate.FuculoKinase));
            AddField(s => ExportToString(s.Isolate.OuterMembraneProteinP6));
            AddField(s => ExportToString(s.Isolate.RealTimePcr));
            AddField(s => ExportToString(s.Isolate.RealTimePcrResult));
            AddField(s => ExportToString(s.Isolate.RibosomalRna16S));
            AddField(s => ExportToString(s.Isolate.RibosomalRna16SBestMatch));
            AddField(s => ExportToString(s.Isolate.RibosomalRna16SMatchInPercent));
            AddField(s => ExportToString(s.Isolate.ApiNh));
            AddField(s => s.Isolate.ApiNhBestMatch);
            AddField(s => s.Isolate.ApiNhMatchInPercent);
            AddField(s => ExportToString(s.Isolate.MaldiTofVitek));
            AddField(s => s.Isolate.MaldiTofVitekBestMatch);
            AddField(s => s.Isolate.MaldiTofVitekMatchConfidence);
            AddField(s => ExportToString(s.Isolate.MaldiTofBiotyper));
            AddField(s => s.Isolate.MaldiTofBiotyperBestMatch);
            AddField(s => s.Isolate.MaldiTofBiotyperMatchConfidence);
            AddField(s => ExportToString(s.Isolate.GenomeSequencing));
            AddField(s => ExportToString(s.Isolate.Ftsi));
            AddField(s => s.Isolate.FtsiEvaluation1);
            AddField(s => s.Isolate.FtsiEvaluation2);
            AddField(s => s.Isolate.FtsiEvaluation3);
            AddField(s => ExportToString(s.Isolate.Mlst));
            AddField(s => s.Isolate.MlstSequenceType);
            AddField(s => ExportToString(s.Isolate.Evaluation));
            AddField(s => s.Isolate.ReportDate);
            AddField(s => s.Isolate.Remark, "Bemerkung (Isolat)");
            AddField(s => ExportChildProperty(s.RkiMatchRecord, rkiMatchRecord => rkiMatchRecord.RkiReferenceId), "RKI InterneRef");
            AddField(s => ExportChildProperty(s.RkiMatchRecord, rkiMatchRecord => rkiMatchRecord.RkiReferenceNumber), "RKI Aktenzeichen");
            AddField(s => ExportChildProperty(s.RkiMatchRecord, rkiMatchRecord => rkiMatchRecord.RkiStatus, ExportToString(RkiStatus.None)), "RKI Status");
        }

        private static string ExportSamplingLocation(SamplingLocation location, Sending sending)
        {
            return location.IsOther() ? sending.OtherSamplingLocation : ExportToString(location);
        }
    }
}