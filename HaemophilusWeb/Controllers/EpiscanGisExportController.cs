using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HaemophilusWeb.Domain;
using HaemophilusWeb.Models;
using HaemophilusWeb.Models.Meningo;
using HaemophilusWeb.Tools;
using HaemophilusWeb.Utils;
using HaemophilusWeb.Views.Utils;

namespace HaemophilusWeb.Controllers
{
    public class EpiscanGisExportController : Controller
    {
        private readonly IApplicationDbContext db = new ApplicationDbContextWrapper(new ApplicationDbContext());


        [HttpGet]
        [Authorize(Roles = DefaultRoles.User)]
        public ActionResult Download(HttpPostedFileBase file)
        {
            var tempFile = Path.GetTempFileName();
            
            var samplingLocations = new List<MeningoSamplingLocation> { MeningoSamplingLocation.Blood, MeningoSamplingLocation.Liquor };

            var sendings = db.MeningoSendings.Include(s => s.Patient)
                .Where(s => !s.Deleted && samplingLocations.Contains(s.SamplingLocation) &&
                            (s.SamplingDate == null && s.ReceivingDate >= new DateTime(2019, 05, 01) ||
                             s.SamplingDate > new DateTime(2019, 05, 01)))
                .OrderBy(s => s.MeningoPatientId).Take(50).ToList();

            /*
                4248;04.10.2019;24.09.2019;B;22;14;5-5;15;48.26385;11.44317
                7327;24.09.2019;12.09.2019;C;;;;24;50.15178;8.7175
                8323;21.05.2019;;B;;;;48;53.92636;9.52543
                8328;24.05.2019;21.05.2019;B;;;;84;49.33616;12.33915
                8329;24.05.2019;20.05.2019;Y;;;;86;48.38987;9.96651
                8330;25.05.2019;20.05.2019;Y;;;;16;52.41826;12.79628
             */

            var csvLines = new List<string>();
            var geo = new GeonamesService();

            foreach (var sending in sendings)
            {
                // TODO
                // Filter out empty Serogroup entries
                // Filter out duplicate patients maybe using exporter
                var interpretation = new MeningoIsolateInterpretation();
                interpretation.Interpret(sending.Isolate);
                var coordinate = geo.QueryCoordinateByPostalCode(sending.Patient.PostalCode, sending.Patient.City);
                FormattableString line =
                    $"{sending.MeningoPatientId};{sending.ReceivingDate.ToReportFormat()};{sending.SamplingDate.ToReportFormat(string.Empty)};{interpretation.Serogroup};{sending.Isolate.PorAVr1};{sending.Isolate.PorAVr2};{sending.Isolate.FetAVr};{sending.Isolate.PatientAgeAtSampling()};{coordinate?.Latitude};{coordinate?.Longitude}";
                csvLines.Add(line.ToString(CultureInfo.InvariantCulture));
            }
            
            //CreateExcelFile.CreateExcelDocument(list, exportDefinition, tempFile, postProcessDataTable);
            System.IO.File.WriteAllLines(tempFile, csvLines);
            var result = File(System.IO.File.ReadAllBytes(tempFile),
                "text/csv",
                $"EpiscanGis_Export_{DateTime.Now:yyyyMMdd}.csv");
            System.IO.File.Delete(tempFile);
            return result;
        }
    }
}