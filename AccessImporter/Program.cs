using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AccessImporter.Converters;
using AutoMapper;
using HaemophilusWeb.Models;
using HaemophilusWeb.Models.Meningo;
using HaemophilusWeb.Utils;
using HaemophilusWeb.Validators;
using HaemophilusWeb.ViewModels;

namespace AccessImporter
{
    class Program
    {
        static void Main(string[] args)
        {
            var connectionString =
                $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={args[0]};Jet OLEDB:Database Password={args[1]}";

            Mapper.Initialize(cfg =>
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

            var selectPatients = "SELECT * FROM Patienten";
            var patientFields = new[]
            {
                "initialen", "geb_dat", "geschlecht", "plz", "wohnort", "bundeslandnr", "meningitis", "sepsis", "wfs",
                "sonst_sympt", "k_sympt", "patnr", "n_spez", "and_inv_erkr","grunderkr", "notizen"

            };
            foreach (var patient in LoadAndConvert<MeningoPatient>(connectionString, selectPatients, patientFields))
            {
                var validator = new MeningoPatientValidator();
                if (validator.Validate(patient).Errors.Any())
                {
                    Console.WriteLine($"Validierung für Patient {patient.PatientId} fehlgeschlagen:");
                    foreach (var error in validator.Validate(patient).Errors)
                    {
                        Console.WriteLine(error.ErrorMessage);
                    }
                }
                Console.WriteLine(patient);
            }


            foreach (var sending in LoadSendings(connectionString))
            {
                var validator = new MeningoSendingValidator();
                if (validator.Validate(sending).Errors.Any())
                {
                    Console.WriteLine("");
                    Console.WriteLine($"Validierung für Patient {sending.MeningoSendingId} fehlgeschlagen:");
                    foreach (var error in validator.Validate(sending).Errors)
                    {
                        Console.WriteLine(error.ErrorMessage);
                    }
                }
                Console.WriteLine(sending);
            }

            foreach (var isolate in LoadIsolates(connectionString))
            {
                Console.WriteLine(isolate);
            }

            Console.ReadLine();
        }

        private static IEnumerable<MeningoSending> LoadSendings(string connectionString)
        {
            var selectSending = "SELECT * FROM Patienten INNER JOIN Staemme ON Patienten.patnr = Staemme.patnr";
            var sendingFields = new[]
            {
                "Patienten.patnr", "Patienten.notizen", "labornr", "art", "eing_dat", "nr_eins", "entn_dat", "isol_mat_nr", "erg_eins"
            };
            Func<MeningoSending, bool> ignoreCasesFromLuxemburg = s => !s.LaboratoryNumber.StartsWith("LX");
            return LoadAndConvert<MeningoSending>(connectionString, selectSending, sendingFields).Where(ignoreCasesFromLuxemburg);
        }

        private static IEnumerable<MeningoIsolate> LoadIsolates(string connectionString)
        {
            var selectIsolates = "SELECT * FROM Patienten INNER JOIN Staemme ON Patienten.patnr = Staemme.patnr";
            var isolateFields = new[]
            {
                "Patienten.patnr", "stammnr", "penicillin", "cefotaxim", "ciprofloxacin", "rifampicin", "rplF", "serogruppe",
                "vr1", "vr2", "Serotyp", "univ_pcr", "sequenz", "Staemme.notizen", "st", "fet-a",
                "cc", "pena", "fHbp", "art"
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