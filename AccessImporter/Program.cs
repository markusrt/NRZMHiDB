﻿using System;
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

            Console.ReadLine();
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