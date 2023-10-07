using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using HaemophilusWeb.Domain;
using HaemophilusWeb.Models;
using HaemophilusWeb.Models.Meningo;
using HaemophilusWeb.Services;
using HaemophilusWeb.Tools;
using HaemophilusWeb.Utils;
using HaemophilusWeb.Views.Utils;

namespace HaemophilusWeb.Controllers
{
    public class EpiscanGisExportController : Controller
    {
        private readonly IApplicationDbContext _db;
        private readonly IGeonamesService _geonamesService;

        public EpiscanGisExportController() : this(new ApplicationDbContextWrapper(new ApplicationDbContext()), new GeonamesService())
        {
        }

        public EpiscanGisExportController(IApplicationDbContext db, IGeonamesService geonamesService)
        {
            _db = db;
            _geonamesService = geonamesService;
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult Index(string token)
        {
            if (token != ConfigurationManager.AppSettings["RegistrationToken"])
            {
                return new HttpNotFoundResult();
            }

            var tempFile = Path.GetTempFileName();
            
            var samplingLocations = new List<MeningoSamplingLocation> { MeningoSamplingLocation.Blood, MeningoSamplingLocation.Liquor };

            var sendings = _db.MeningoSendings.Include(s => s.Patient)
                .Where(s => !s.Deleted && samplingLocations.Contains(s.SamplingLocation) &&
                            (s.SamplingDate == null && s.ReceivingDate >= new DateTime(2019, 10, 01) ||
                             s.SamplingDate > new DateTime(2019, 10, 01)))
                .OrderBy(s => s.MeningoPatientId).ToList();

            var patientLines = new Dictionary<int, string>();

            foreach (var sending in sendings)
            {
                var interpretation = new MeningoIsolateInterpretation();
                interpretation.Interpret(sending.Isolate);

                var serogroup = interpretation.Serogroup;

                if (string.IsNullOrEmpty(serogroup) || serogroup is "NG" or "cnl")
                {
                    continue;
                }

                var coordinate = _geonamesService.QueryCoordinateByPostalCode(sending.Patient.PostalCode, sending.Patient.City);
                if (coordinate == null)
                {
                    continue;
                }
                var patientId = sending.MeningoPatientId;
                FormattableString line =
                    $"{patientId};{sending.ReceivingDate.ToReportFormat()};{sending.SamplingDate.ToReportFormat(string.Empty)};{interpretation.Serogroup};{sending.Isolate.PorAVr1};{sending.Isolate.PorAVr2};{sending.Isolate.FetAVr};{sending.Isolate.PatientAgeAtSampling()};{coordinate?.Latitude};{coordinate?.Longitude}";
                var newLine = line.ToString(CultureInfo.InvariantCulture);

                if (!patientLines.ContainsKey(patientId) ||
                    patientLines[patientId].Length < newLine.Length)
                {
                    patientLines[patientId] = newLine;
                }
            }
            
            System.IO.File.WriteAllLines(tempFile, patientLines.Values);
            var result = File(System.IO.File.ReadAllBytes(tempFile),
                "text/csv",
                $"EpiscanGis_Export_{DateTime.Now:yyyyMMdd}.csv");
            System.IO.File.Delete(tempFile);
            return result;
        }
    }
}