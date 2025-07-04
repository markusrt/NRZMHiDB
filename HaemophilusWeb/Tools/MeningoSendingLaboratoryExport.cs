﻿using System;
using HaemophilusWeb.Domain;
using HaemophilusWeb.Models;
using HaemophilusWeb.Models.Meningo;
using HaemophilusWeb.Utils;
using HaemophilusWeb.Views.Utils;
using static HaemophilusWeb.Services.PubMlstService;

namespace HaemophilusWeb.Tools
{
    public class MeningoSendingLaboratoryExport : SendingExportDefinition<MeningoSending, MeningoPatient>
    {
        private readonly MeningoIsolateInterpretation _isolateInterpretation = new MeningoIsolateInterpretation();

        public MeningoSendingLaboratoryExport()
        {
            AddField(s => s.MeningoPatientId, "Patienten-Nr.");
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
            AddField(s => s.SerogroupSender);

            AddField(s => s.Patient.Initials);
            AddField(s => s.Patient.BirthDate.ToReportFormat());
            AddField(s => s.Isolate.PatientAgeAtSampling(), "Patientenalter bei Entnahme");
            AddField(s => ExportToString(s.Patient.Gender));
            AddField(s => s.Patient.PostalCode);
            AddField(s => s.Patient.City);
            AddField(s => s.Patient.County);
            AddField(s => ExportToString(s.Patient.State));
            AddField(s => ExportClinicalInformation(s.Patient.ClinicalInformation, () => s.Patient.OtherClinicalInformation, _ => _.HasFlag(MeningoClinicalInformation.Other)));
            AddField(s => ExportRiskFactors(s.Patient.RiskFactors, () => s.Patient.OtherRiskFactor, _ => _.HasFlag(RiskFactors.Other)));
            AddField(s => s.Remark, "Bemerkung (Einsendung)");

            AddField(s => DetectInterpretationRule(s.Isolate), "Regel");
            AddField(s => DetectSerogroup(s.Isolate), "Serogruppe");
            AddField(s => DetectMeningococci(s.Isolate), "Meningokokken");
            AddField(s => ExportToString(s.Isolate.GrowthOnBloodAgar));
            AddField(s => ExportToString(s.Isolate.GrowthOnMartinLewisAgar));
            AddField(s => ExportToString(s.Isolate.Oxidase));
            AddField(s => ExportToString(s.Isolate.Onpg));
            AddField(s => ExportToString(s.Isolate.GammaGt));
            AddField(s => ExportToString(s.Isolate.SiaAGene));
            AddField(s => ExportToString(s.Isolate.CapsularTransferGene));
            AddField(s => ExportToString(s.Isolate.CapsuleNullLocus));
            AddField(s => ExportToString(s.Isolate.Agglutination));
            AddField(s => ExportToString(s.Isolate.SerogroupPcr));
            
            AddFieldOnPositiveTestResult(s => s.Isolate.PorAPcr, s => s.Isolate.PorAVr1, "PorA-VR1");
            AddFieldOnPositiveTestResult(s => s.Isolate.PorAPcr, s => s.Isolate.PorAVr2, "PorA-VR2");
            AddFieldOnPositiveTestResult(s => s.Isolate.FetAPcr, s => s.Isolate.FetAVr, "FetA-VR");

            AddField(s => ExportToString(s.Isolate.CsbPcr));
            AddField(s => ExportToString(s.Isolate.CscPcr));
            AddField(s => ExportToString(s.Isolate.CswyPcr));
            AddField(s => ExportToString(s.Isolate.CswyAllele));
            AddField(s => ExportToString(s.Isolate.RealTimePcr));
            AddField(s => ExportToString(s.Isolate.RealTimePcrResult));
            AddField(s => ExportToString(s.Isolate.RibosomalRna16S));
            AddField(s => ExportToString(s.Isolate.RibosomalRna16SBestMatch));
            AddField(s => ExportToString(s.Isolate.RibosomalRna16SMatchInPercent));
            AddField(s => ExportToString(s.Isolate.MaldiTofVitek));
            AddField(s => s.Isolate.MaldiTofVitekBestMatch);
            AddField(s => s.Isolate.MaldiTofVitekMatchConfidence);
            AddField(s => ExportToString(s.Isolate.MaldiTofBiotyper));
            AddField(s => s.Isolate.MaldiTofBiotyperBestMatch);
            AddField(s => s.Isolate.MaldiTofBiotyperMatchConfidence);
            AddField(s => s.Isolate.ReportDate);
            AddField(s => s.Isolate.Remark, "Bemerkung (Isolat)");

            AddEpsilometerTestFields(this, Antibiotic.Benzylpenicillin);
            AddEpsilometerTestFields(this, Antibiotic.Ciprofloxacin);
            AddEpsilometerTestFields(this, Antibiotic.Rifampicin);
            AddEpsilometerTestFields(this, Antibiotic.Cefotaxime);
            AddEpsilometerTestFields(this, Antibiotic.Azithromycin);

            AddPubMsltProperty("PubMLST ID", _ => _.PubMlstId, "-");
            AddPubMsltProperty("Datenbank", _ => _.Database);
            AddPubMsltProperty(SequenceType, _ => _.SequenceType);
            AddPubMsltProperty(ClonalComplex, _ => _.ClonalComplex);
            AddPubMsltProperty(PorAVr1, _ => _.PorAVr1);
            AddPubMsltProperty(PorAVr2, _ => _.PorAVr2);
            AddPubMsltProperty(FetAVr, _ => _.FetAVr);
            AddPubMsltProperty(PorB, _ => _.PorB);
            AddPubMsltProperty(NhbaPeptide, _ => _.Nhba);
            AddPubMsltProperty(PenA, _ => _.PenA);
            AddPubMsltProperty(GyrA, _ => _.GyrA);
            AddPubMsltProperty("parC", _ => _.ParC);
            AddPubMsltProperty(RpoB, _ => _.RpoB);
            AddPubMsltProperty(RplF, _ => _.RplF);
            AddPubMsltProperty(Fhbp, _ => _.Fhbp);
            AddPubMsltProperty("parE", _ => _.ParE);
            AddPubMsltProperty(BexseroReactivity, _ => _.BexseroReactivity);
            AddPubMsltProperty(TrumenbaReactivity, _ => _.TrumenbaReactivity);
        }

        private string DetectSerogroup(MeningoIsolate isolate)
        {
            _isolateInterpretation.Interpret(isolate);
            return _isolateInterpretation.Serogroup;
        }

        private string DetectMeningococci(MeningoIsolate isolate)
        {
            _isolateInterpretation.Interpret(isolate);
            return _isolateInterpretation.NoMeningococci ? "kein Nachweis" : null;
        }

        private string DetectInterpretationRule(MeningoIsolate isolate)
        {
            _isolateInterpretation.Interpret(isolate);
            return _isolateInterpretation.Rule;
        }

        private string ExportRiskFactors<T>(T clinicalInformation, Func<string> otherClinicalInformation, Func<T, bool> isOther)
        {
            var riskFactors = EnumEditor.GetEnumDescription(clinicalInformation);
            if (isOther(clinicalInformation))
            {
                riskFactors = riskFactors.Replace(
                    EnumEditor.GetEnumDescription(RiskFactors.Other), otherClinicalInformation());
            }
            return riskFactors;
        }

        private void AddPubMsltProperty(string header, Func<NeisseriaPubMlstIsolate, object> property, string nullValue=null)
        {
            AddField(s => ExportChildProperty(
                s.Isolate.NeisseriaPubMlstIsolate, property, nullValue), header);
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