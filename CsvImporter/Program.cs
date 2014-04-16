using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text;
using HaemophilusWeb.Models;
using ServiceStack;
using ServiceStack.Text;

namespace CsvImporter
{
    internal class Program
    {
        private static readonly ApplicationDbContext Db = new ApplicationDbContext();

        private static void Main(string[] args)
        {
            var csvRecords = new List<SendingCsvRecord>();
            var sendings =
                Db.Sendings.Include(s => s.Patient)
                    .Include(s => s.Isolate)
                    .Include(s => s.Isolate.EpsilometerTests.Select(e => e.EucastClinicalBreakpoint));
            foreach (var sending in sendings)
            {
                var csvRecord = new SendingCsvRecord();
                csvRecord.PopulateWith(sending);
                csvRecord.PopulateWith(sending.Patient);
                csvRecord.PopulateWith(sending.Isolate);

                PopulateEpsilometerTestResult(sending, Antibiotic.Ampicillin,
                    _ => csvRecord.AmpicillinEpsilometerTestResult = _,
                    _ => csvRecord.AmpicillinMeasurement = _);

                PopulateEpsilometerTestResult(sending, Antibiotic.AmoxicillinClavulanate,
                    _ => csvRecord.AmoxicillinClavulanateEpsilometerTestResult = _,
                    _ => csvRecord.AmoxicillinClavulanateMeasurement = _);

                PopulateEpsilometerTestResult(sending, Antibiotic.Cefotaxime,
                    _ => csvRecord.CefotaximeEpsilometerTestResult = _,
                    _ => csvRecord.CefotaximeMeasurement = _);

                PopulateEpsilometerTestResult(sending, Antibiotic.Meropenem,
                    _ => csvRecord.MeropenemEpsilometerTestResult = _,
                    _ => csvRecord.MeropenemMeasurement = _);
                csvRecord.EuCastVersion = "3.1";
                csvRecords.Add(csvRecord);
            }
            var sendingsCsv = CsvSerializer.SerializeToCsv(csvRecords);

            File.WriteAllText(@"c:\temp\import.csv", sendingsCsv, Encoding.GetEncoding("UTF-8"));
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        private static void PopulateEpsilometerTestResult(Sending sending, Antibiotic antibiotic,
            Action<EpsilometerTestResult> populateTestResult, Action<double> populateMeasurement)
        {
            var eTestResult = sending.Isolate.EpsilometerTests.SingleOrDefault(
                e => e.EucastClinicalBreakpoint.Antibiotic == antibiotic);
            if (eTestResult == null)
            {
                return;
            }
            populateTestResult(eTestResult.Result);
            populateMeasurement(eTestResult.Measurement);
        }
    }
}