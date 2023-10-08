using System;
using System.Collections.Generic;
using System.Linq;
using HaemophilusWeb.Models;
using HaemophilusWeb.Utils;

namespace HaemophilusWeb.Tools
{
    public class HaemophilusSendingRkiExport : SendingExportDefinition<Sending, Patient>
    {
        public HaemophilusSendingRkiExport(List<County> counties)
        {
            var col = new RkiExportColumns();
            var emptyCounty = new County { CountyNumber = "" };
            Func<Sending, County> findCounty =
                s => counties.FirstOrDefault(c => c.IsEqualTo(s.Patient.County)) ?? emptyCounty;

            
            AddField(s => s.Patient.Initials, col.Initials);
            AddField(s => s.Patient.BirthDate, col.DateOfBirth);
            AddField(s => s.Patient.PostalCode, col.PostalCode);

            AddField(s => s.Isolate.StemNumber ?? -1, col.StemNumber);
            AddField(s => s.ReceivingDate.ToReportFormat(), col.ReceivedDate);
            AddField(s => s.SamplingDate.ToReportFormat(), col.SampleDate);
            AddField(s => ExportSamplingLocation(s.SamplingLocation, s), col.Source);
            AddField(s => s.Patient.BirthDate.HasValue ? s.Patient.BirthDate.Value.Month : 0, col.BirthMonth);
            AddField(s => s.Patient.BirthDate.HasValue ? s.Patient.BirthDate.Value.Year : 0, col.BirthYear);
            AddField(s => ExportGender(s.Patient.Gender), col.Sex);
            AddField(s => ExportToString(s.Patient.HibVaccination), col.HibVaccination);
            AddField(s => ExportToString(s.Isolate.Evaluation), col.Serotype);
            AddField(s => ExportToString(s.Isolate.BetaLactamase), col.BetaLactamase);
            AddField(s => findCounty(s).CountyNumber, col.CountyNumber);
            AddField(s => new string(findCounty(s).CountyNumber.Take(2).ToArray()), col.StateNumber);
            AddField(s => s.SenderId, col.SenderId);
            AddField(s => ExportToString(s.Patient.County), col.County);
            AddField(s => ExportToString(s.Patient.State), col.State);
            AddEpsilometerTestFields(this, Antibiotic.Ampicillin, false, col.AmxMic, col.AmxSir);
            AddEpsilometerTestFields(this, Antibiotic.AmoxicillinClavulanate, false, col.AmcMic, col.AmcSir);

        }

        private static string ExportSamplingLocation(SamplingLocation location, Sending sending)
        {
            return location.IsOther() ? sending.OtherSamplingLocation : ExportToString(location);
        }
    }
}