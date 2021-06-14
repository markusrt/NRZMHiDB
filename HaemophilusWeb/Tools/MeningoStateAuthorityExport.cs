using System;
using System.Collections.Generic;
using System.Linq;
using HaemophilusWeb.Models;
using HaemophilusWeb.Models.Meningo;
using HaemophilusWeb.Utils;
using NLog;

namespace HaemophilusWeb.Tools
{
    public class MeningoStateAuthorityExport : SendingExportDefinition<MeningoSending, MeningoPatient>
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public MeningoStateAuthorityExport(IReadOnlyCollection<County> counties)
        {
            var emptyCounty = new County { CountyNumber = "" };
            Func<MeningoSending, County> findCounty =
                s =>
                {
                    var county = counties.FirstOrDefault(c => c.IsEqualTo(s.Patient.County)) ?? emptyCounty;
                    if (!string.IsNullOrEmpty(s.Patient.County) && county == emptyCounty)
                    {
                        Log.Warn($"County {s.Patient.County} not found for sending {s.MeningoSendingId}.");
                    }
                    return county;
                };


            AddField(s => s.MeningoPatientId, "Patientnr NRZM");
            AddField(s => s.Patient.Gender.HasValue ? ExportToString(s.Patient.Gender).Substring(0,1) : null, "Geschlecht");
            AddField(s => s.Patient.BirthDate.ToReportFormatMonthYear(), "Geburtsmonat");
            AddField(s => s.ReceivingDate.ToReportFormat());
            AddField(s => s.SamplingDate.ToReportFormat());
            AddField(s => ExportToString(s.Isolate.Agglutination), "Serogruppe");
            
            AddFieldOnPositiveTestResult(s => s.Isolate.PorAPcr, s => s.Isolate.PorAVr1, "PorA VR1");
            AddFieldOnPositiveTestResult(s => s.Isolate.PorAPcr, s => s.Isolate.PorAVr2, "PorA VR2");
            AddFieldOnPositiveTestResult(s => s.Isolate.FetAPcr, s => s.Isolate.FetAVr, "FetA VR");

            AddField(s => FindSpecies(s.Isolate), "Spezies");
            AddField(s => findCounty(s).CountyNumber, "Landkreis");
            AddField(s => new string(findCounty(s).CountyNumber.Take(2).ToArray()), "Bundesland");
        }

        private static string FindSpecies(MeningoIsolate isolate)
        {
            var possibleSpecies = new List<string> {isolate.MaldiTofBestMatch, isolate.RibosomalRna16SBestMatch};
            if (isolate.RealTimePcr == NativeMaterialTestResult.Positive)
            {
                possibleSpecies.Add(EnumUtils.GetEnumDescription<RealTimePcrResult>(isolate.RealTimePcrResult));
            }
            return possibleSpecies.FirstOrDefault(s => !string.IsNullOrEmpty(s));
        }
    }
}