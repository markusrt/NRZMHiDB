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
            AddField(s => ExportToString(s.Isolate.PorAVr1), "PorA VR1");
            AddField(s => ExportToString(s.Isolate.PorAVr2), "PorA VR2");
            AddField(s => ExportToString(s.Isolate.FetAVr), "FetA VR");

            AddEpsilometerTestFields(this, Antibiotic.Benzylpenicillin, true, "Penicillin MHK Etest");
            AddEpsilometerTestFields(this, Antibiotic.Ciprofloxacin, true, "Ciprofloxacin MHK Etest");
            AddEpsilometerTestFields(this, Antibiotic.Cefotaxime, true, "Cefotaxim MHK Etest");
            AddEpsilometerTestFields(this, Antibiotic.Rifampicin, true, "Rifampicin MHK Etest");

            AddField(s => s.Isolate.NeisseriaPubMlstIsolate == null ? null : s.Isolate.NeisseriaPubMlstIsolate.PenA, PenA);
            AddField(s => s.Isolate.NeisseriaPubMlstIsolate == null ? null : s.Isolate.NeisseriaPubMlstIsolate.GyrA, GyrA);
            AddField(s => s.Isolate.NeisseriaPubMlstIsolate == null ? null : s.Isolate.NeisseriaPubMlstIsolate.RpoB, RpoB);
            AddField(s => s.Isolate.NeisseriaPubMlstIsolate == null ? null : s.Isolate.NeisseriaPubMlstIsolate.SequenceType, SequenceType);
            AddField(s => s.Isolate.NeisseriaPubMlstIsolate == null ? null : s.Isolate.NeisseriaPubMlstIsolate.ClonalComplex, ClonalComplex);

            AddField(s => s.Patient.BirthDate.HasValue ? s.Patient.BirthDate.Value.Month : 0, "Geburtsmonat");
            AddField(s => s.Patient.BirthDate.HasValue ? s.Patient.BirthDate.Value.Year : 0, "Geburtsjahr");
            AddField(s => ExportGender(s.Patient.Gender), "Geschlecht");

            AddField(s => findCounty(s).CountyNumber, "Landkreis");
            AddField(s => new string(findCounty(s).CountyNumber.Take(2).ToArray()), "Bundesland");
        }


        private static string ExportSamplingLocation(SamplingLocation location, Sending sending)
        {
            if (location == SamplingLocation.Other)
            {
                return sending.OtherSamplingLocation;
            }
            return ExportToString(location);
        }
    }
}