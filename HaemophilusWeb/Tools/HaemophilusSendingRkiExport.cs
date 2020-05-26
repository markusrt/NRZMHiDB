using System;
using System.Collections.Generic;
using System.Linq;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using HaemophilusWeb.Domain;
using HaemophilusWeb.Models;
using HaemophilusWeb.Models.Meningo;
using HaemophilusWeb.Utils;
using static HaemophilusWeb.Services.PubMlstService;

namespace HaemophilusWeb.Tools
{
    public class HaemophilusSendingRkiExport : SendingExportDefinition<Sending, Patient>
    {
        public HaemophilusSendingRkiExport(List<County> counties)
        {
            var emptyCounty = new County { CountyNumber = "" };
            Func<Sending, County> findCounty =
                s => counties.FirstOrDefault(c => c.IsEqualTo(s.Patient.County)) ?? emptyCounty;

            AddField(s => s.Isolate.StemNumber, "klhi_nr");
            AddField(s => s.ReceivingDate.ToReportFormat(), "eing");
            AddField(s => s.SamplingDate.ToReportFormat(), "ent");
            AddField(s => ExportSamplingLocation(s.SamplingLocation, s), "mat");
            AddField(s => s.Patient.BirthDate.HasValue ? s.Patient.BirthDate.Value.Month : 0, "geb_monat");
            AddField(s => s.Patient.BirthDate.HasValue ? s.Patient.BirthDate.Value.Year : 0, "geb_jahr");
            AddField(s => ExportGender(s.Patient.Gender), "geschlecht");
            AddField(s => ExportToString(s.Patient.HibVaccination), "hib_impf");
            AddField(s => ExportToString(s.Isolate.Evaluation), "styp");
            AddField(s => ExportToString(s.Isolate.BetaLactamase), "b_lac");
            AddField(s => findCounty(s).CountyNumber, "kreis_nr");
            AddField(s => new string(findCounty(s).CountyNumber.Take(2).ToArray()), "bundesland");
            AddField(s => s.SenderId, "einsender");
            AddField(s => ExportToString(s.Patient.County), "landkreis");
            AddField(s => ExportToString(s.Patient.State), "bundeslandName");
            AddEpsilometerTestFields(this, Antibiotic.Ampicillin, false, "ampicillinMHK", "ampicillinBewertung");
            AddEpsilometerTestFields(this, Antibiotic.AmoxicillinClavulanate, false, "amoxicillinClavulansaeureMHK", "bewertungAmoxicillinClavulansaeure");

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