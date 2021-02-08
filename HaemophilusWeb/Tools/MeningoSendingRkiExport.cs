using System;
using System.Collections.Generic;
using System.Linq;
using HaemophilusWeb.Models;
using HaemophilusWeb.Models.Meningo;
using HaemophilusWeb.Utils;
using static HaemophilusWeb.Services.PubMlstService;

namespace HaemophilusWeb.Tools
{
    public class MeningoSendingRkiExport : SendingExportDefinition<MeningoSending, MeningoPatient>
    {
        public MeningoSendingRkiExport(List<County> counties)
        {
            var emptyCounty = new County { CountyNumber = "" };
            Func<MeningoSending, County> findCounty =
                s => counties.FirstOrDefault(c => c.IsEqualTo(s.Patient.County)) ?? emptyCounty;

            AddField(s => s.Patient.PatientId, "PatNr NRZM");
            AddField(s => s.ReceivingDate.ToReportFormat(), "Eingang NRZM");
            AddField(s => s.SamplingDate.ToReportFormat(), "Entnahme");
            AddField(s => ExportToString(s.Isolate.CapsuleNullLocus), "cnl");
            AddField(s => ExportToString(s.Isolate.Agglutination), "Agglutination");
            AddField(s => ExportToString(s.Isolate.SerogroupPcr), "Serogruppen-PCR");
            AddField(s => ExportToString(s.Isolate.CsbPcr), "csb-PCR");
            AddField(s => ExportToString(s.Isolate.CscPcr), "csc-PCR");
            AddField(s => ExportToString(s.Isolate.CswyPcr), "cswy-PCR");
            AddField(s => ExportToString(s.Isolate.CswyAllele), "cswy-Allel");
            AddField(s => ExportToString(s.Isolate.RealTimePcr));
            AddField(s => ExportToString(s.Isolate.RealTimePcrResult));
            AddPubMsltProperty(PorAVr1, _ => _.PorAVr1);
            AddPubMsltProperty(PorAVr2, _ => _.PorAVr2);
            AddPubMsltProperty(FetAVr, _ => _.FetAVr);

            AddEpsilometerTestFields(this, Antibiotic.Benzylpenicillin, true, "Penicillin MHK Etest");
            AddEpsilometerTestFields(this, Antibiotic.Ciprofloxacin, true, "Ciprofloxacin MHK Etest");
            AddEpsilometerTestFields(this, Antibiotic.Cefotaxime, true, "Cefotaxim MHK Etest");
            AddEpsilometerTestFields(this, Antibiotic.Rifampicin, true, "Rifampicin MHK Etest");

            AddPubMsltProperty(PenA, _ => _.PenA);
            AddPubMsltProperty(GyrA, _ => _.GyrA);
            AddPubMsltProperty(RpoB, _ => _.RpoB);
            AddPubMsltProperty(SequenceType, _ => _.SequenceType);
            AddPubMsltProperty(ClonalComplex, _ => _.ClonalComplex);

            AddField(s => s.Patient.BirthDate.HasValue ? s.Patient.BirthDate.Value.Month : 0, "Geburtsmonat");
            AddField(s => s.Patient.BirthDate.HasValue ? s.Patient.BirthDate.Value.Year : 0, "Geburtsjahr");
            AddField(s => ExportGender(s.Patient.Gender), "Geschlecht");

            AddField(s => findCounty(s).CountyNumber, "Landkreis");
            AddField(s => new string(findCounty(s).CountyNumber.Take(2).ToArray()), "Bundesland");
        }

        private void AddPubMsltProperty(string header, Func<NeisseriaPubMlstIsolate, object> property, string nullValue=null)
        {
            AddField(s => ExportChildProperty(
                s.Isolate.NeisseriaPubMlstIsolate, property, nullValue), header);
        }
    }
}