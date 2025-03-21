using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AccessImporter.Converters;
using AutoMapper;
using HaemophilusWeb.Automapper;
using HaemophilusWeb.Migrations;
using HaemophilusWeb.Models;
using HaemophilusWeb.Models.Meningo;
using HaemophilusWeb.Utils;
using HaemophilusWeb.Validators;
using HaemophilusWeb.ViewModels;
using Sending = HaemophilusWeb.Migrations.Sending;

namespace AccessImporter
{
    class Program
    {
        public static IMapper Mapper { get; private set; }

        public static ApplicationDbContext Context = new ApplicationDbContext();

        public static string StemAccessTable = "Staemme_2019";
        public static string PatientAccessTable = "Patienten_2019";

        static void Main(string[] args)
        {
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<ApplicationDbContext, Configuration>());

            var connectionString =
                $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={args[0]};Jet OLEDB:Database Password={args[1]}";


            var mapperConfiguration = new MapperConfiguration(cfg =>
            {                
                cfg.CreateMap<object, MeningoSamplingLocation>()
                    .ConvertUsing<AccessMeningoSamplingLocationConverter>();
                cfg.CreateMap<Dictionary<string, object>, MeningoPatient>()
                    .ConvertUsing<AccessMeningoPatientConverter>();
                cfg.CreateMap<Dictionary<string, object>, MeningoSending>()
                    .ConvertUsing<AccessMeningoSendingConverter>();
                cfg.CreateMap<Dictionary<string, object>, MeningoIsolate>()
                    .ConvertUsing<AccessMeningoIsolateConverter>();
            });

            var selectSamplingLocations = "SELECT * FROM tbl_isol_mat";
            foreach (var meningoSamplingLocation in LoadAndConvert<MeningoSamplingLocation>(connectionString, selectSamplingLocations, "isol_mat_nr"))
            {
                Console.WriteLine(meningoSamplingLocation);
            }

            var selectPatients =
                $"SELECT {PatientAccessTable}.* FROM {PatientAccessTable} INNER JOIN {StemAccessTable} ON {PatientAccessTable}.patnr = {StemAccessTable}.patnr"; // "WHERE Year(eing_dat)=2019";
            var patientFields = new[]
            {
                "initialen", "geb_dat", "geschlecht", "plz", "wohnort", "bundeslandnr", "meningitis", "sepsis", "wfs",
                "sonst_sympt", "k_sympt", "patnr", "n_spez", "and_inv_erkr","grunderkr", "notizen"

            };

            Dictionary<int, int> patientIdLookup = new Dictionary<int, int>();


            var patients = LoadAndConvert<MeningoPatient>(connectionString, selectPatients, patientFields).ToList();
            var validationFailed = false;
            foreach (var patient in patients)
            {
                var validator = new MeningoPatientValidator();
                if (validator.Validate(patient).Errors.Any())
                {
                    Console.WriteLine($"Validierung für Patient {patient.PatientId} fehlgeschlagen:");
                    foreach (var error in validator.Validate(patient).Errors)
                    {
                        Console.WriteLine(error.ErrorMessage);
                        validationFailed = true;
                    }
                }
            }

            var sendings = LoadSendings(connectionString).ToList();
            foreach (var sending in sendings)
            {
                var validator = new MeningoSendingValidator();
                if (validator.Validate(sending).Errors.Any())
                {
                    Console.WriteLine("");
                    Console.WriteLine($"Validierung für Einsendung {sending.MeningoSendingId} fehlgeschlagen:");
                    foreach (var error in validator.Validate(sending).Errors)
                    {
                        Console.WriteLine(error.ErrorMessage);
                        if (error.ErrorMessage.Contains("Anderer Entnahmeort") && error.ErrorMessage.Contains("darf nicht leer sein"))
                        {
                            continue;
                        }
                        validationFailed = true;
                    }
                }
            }

            if (validationFailed)
            {
                return;
            }

            var currentSeed = Context.MeningoPatients.Max(p => p.PatientId);

            foreach (var patient in patients)
            {
                var oldId = patient.PatientId;

                if (!patientIdLookup.ContainsKey(oldId))
                {

                    Context.Database.ExecuteSqlCommand(String.Format("DBCC CHECKIDENT ([MeningoPatients], RESEED, {0})", patient.PatientId - 1));

                    Console.WriteLine(patient);
                    Context.MeningoPatients.Add(patient);
                    new ApplicationDbContextWrapper(Context).SaveChanges();
                    if (patient.PatientId != oldId)
                    {
                        Console.WriteLine($"ERROR: Patient ID mismatch old: {oldId}, new: {patient.PatientId}");
                        return;
                    }
                    patientIdLookup.Add(oldId, patient.PatientId);
                }
            }

            Context.Database.ExecuteSqlCommand(String.Format("DBCC CHECKIDENT ([MeningoPatients], RESEED, {0})", currentSeed));

            var sendingLookup = new Dictionary<int, MeningoSending>();

            foreach (var sending in sendings)
            {
                var oldId = sending.MeningoSendingId;

                sendingLookup.Add(oldId, sending);
                //Console.WriteLine(sending);
            }

            var currentIsolateSeed = Context.MeningoIsolates.Max(i => i.MeningoIsolateId);
            var currentSendingSeed = Context.MeningoSendings.Max(s => s.MeningoSendingId);

            foreach (var isolate in LoadIsolates(connectionString))
            {
                var sending = sendingLookup[isolate.MeningoSendingId];
                isolate.Sending = sending;
                isolate.Sending.Patient = Context.MeningoPatients.Find(sending.MeningoPatientId);
                if (Regex.Match(sending.LaboratoryNumber, "NR\\d+/\\d+").Success)
                {
                    var noPrefix = sending.LaboratoryNumber.Replace("NR", "");
                    var labNumberAndYear = noPrefix.Split('/');
                    var remark = "Nativmaterial: " + sending.LaboratoryNumber;
                    isolate.YearlySequentialIsolateNumber = (-1) * int.Parse(labNumberAndYear[0]);
                    isolate.Year = 2000 + int.Parse(labNumberAndYear[1]);
                    isolate.Remark = string.IsNullOrEmpty(isolate.Remark) ? remark : "\n" + remark;

                    // Special handling for SerogroupPCR on native material
                    var serogroupPcr = isolate.SerogroupPcr;
                    if (serogroupPcr != MeningoSerogroupPcr.NotDetermined)
                    {
                        switch (serogroupPcr)
                        {
                            case MeningoSerogroupPcr.B:
                                isolate.CsbPcr = NativeMaterialTestResult.Positive;
                                break;
                            case MeningoSerogroupPcr.C:
                                isolate.CscPcr = NativeMaterialTestResult.Positive;
                                break;
                            case MeningoSerogroupPcr.W:
                                isolate.CswyPcr = NativeMaterialTestResult.Positive;
                                isolate.CswyAllele = CswyAllel.Allele1;
                                break;
                            case MeningoSerogroupPcr.Y:
                                isolate.CswyPcr = NativeMaterialTestResult.Positive;
                                isolate.CswyAllele = CswyAllel.Allele2;
                                break;
                            case MeningoSerogroupPcr.WY:
                                isolate.CswyPcr = NativeMaterialTestResult.Positive;
                                isolate.CswyAllele = CswyAllel.Allele3;
                                break;
                            default:
                                throw new Exception("Unexpected Serogroup PCR for native material");
                        }
                        isolate.SerogroupPcr = MeningoSerogroupPcr.NotDetermined;
                    }
                }
                else if (Regex.Match(sending.LaboratoryNumber, "\\d+/\\d+").Success)
                {
                    var noPrefix = sending.LaboratoryNumber.Replace("MZ", "");
                    var labNumberAndYear = noPrefix.Split('/');
                    isolate.YearlySequentialIsolateNumber = int.Parse(labNumberAndYear[0]);
                    isolate.Year = 2000 + int.Parse(labNumberAndYear[1]);
                }
                var mapping = new MeningoIsolateViewModelMappingAction();
                var viewModel = new MeningoIsolateViewModel();
                mapping.Process(isolate, viewModel, null);
                var validator = new MeningoIsolateViewModelValidator();

                if (validator.Validate(viewModel).Errors.Any())
                {
                    Console.WriteLine("");
                    Console.WriteLine($"Validierung für Isolat {isolate.LaboratoryNumber} fehlgeschlagen:");
                    foreach (var error in validator.Validate(viewModel).Errors)
                    {
                        Console.WriteLine(error.ErrorMessage);
                    }
                }

                if (isolate.StemNumber.HasValue && isolate.EpsilometerTests.Any())
                {
                    isolate.GrowthOnBloodAgar = Growth.TypicalGrowth;
                    isolate.GrowthOnMartinLewisAgar = Growth.TypicalGrowth;
                }

                Context.Database.ExecuteSqlCommand(String.Format("DBCC CHECKIDENT ([MeningoIsolates], RESEED, {0})", isolate.MeningoSendingId - 1));
                Context.Database.ExecuteSqlCommand(String.Format("DBCC CHECKIDENT ([MeningoSendings], RESEED, {0})", isolate.MeningoSendingId - 1));

                Context.MeningoIsolates.Add(isolate);
                new ApplicationDbContextWrapper(Context).SaveChanges();
                //Console.WriteLine(isolate);
            }

            Context.Database.ExecuteSqlCommand(String.Format("DBCC CHECKIDENT ([MeningoIsolates], RESEED, {0})", currentIsolateSeed));
            Context.Database.ExecuteSqlCommand(String.Format("DBCC CHECKIDENT ([MeningoSendings], RESEED, {0})", currentSendingSeed));

            Console.ReadLine();
        }

        private static IEnumerable<MeningoSending> LoadSendings(string connectionString)
        {
            var selectSending =
                $"SELECT * FROM {PatientAccessTable} INNER JOIN {StemAccessTable} ON {PatientAccessTable}.patnr = {StemAccessTable}.patnr"; //" WHERE Year(eing_dat)=2019";
            var sendingFields = new[]
            {
                $"{PatientAccessTable}.patnr", $"{PatientAccessTable}.notizen", "dbnr", "labornr", "art", "eing_dat", "nr_eins", "entn_dat", "isol_mat_nr", "erg_eins"
            };
            Func<MeningoSending, bool> ignoreCasesFromLuxemburg = s => !s.LaboratoryNumber.StartsWith("LX");
            return LoadAndConvert<MeningoSending>(connectionString, selectSending, sendingFields).Where(ignoreCasesFromLuxemburg);
        }

        private static IEnumerable<MeningoIsolate> LoadIsolates(string connectionString)
        {
            var selectIsolates =
                $"SELECT * FROM {PatientAccessTable} INNER JOIN {StemAccessTable} ON {PatientAccessTable}.patnr = {StemAccessTable}.patnr"; //" WHERE Year(eing_dat)=2019";
            var isolateFields = new[]
            {
                $"{PatientAccessTable}.patnr", "dbnr", "stammnr", "penicillin", "cefotaxim", "ciprofloxacin", "rifampicin", "rplF", "serogruppe", "serogruppe_PCR", "NHS",
                "vr1", "vr2", "Serotyp", "univ_pcr", "sequenz", $"{StemAccessTable}.notizen", "st", "fet-a",
                "cc", "pena", "fHbp", "art", "eing_dat"
            };
            return LoadAndConvert<MeningoIsolate>(connectionString, selectIsolates, isolateFields);
        }

        private static IEnumerable<T> LoadAndConvert<T>(string connectionString, string query, params string[] columns)
        {
            using (var connection = new OleDbConnection(connectionString))
            {
                var command = new OleDbCommand(query, connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var result = default(T);
                        try
                        {
                            if (columns.Length == 1)
                            {
                                result = Mapper.Map<T>(reader[columns[0]]);
                            }
                            else
                            {
                                result = Mapper.Map<T>(MapRowToDictionary(reader, columns));
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                        }
                        yield return result;
                    }
                }
            }
        }

        private static Dictionary<string, object> MapRowToDictionary(OleDbDataReader row, params string[] columns)
        {
            var dictionary = new Dictionary<string, object>();
            foreach (var column in columns)
            {
                dictionary[column] = row[column];
            }

            return dictionary;
        }
    }
}