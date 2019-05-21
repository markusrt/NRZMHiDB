using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using HaemophilusWeb.Models;
using HaemophilusWeb.Models.Meningo;
using HaemophilusWeb.Utils;
using Newtonsoft.Json.Linq;

namespace AccessImporter.Converters
{
    public class AccessMeningoPatientConverter : ITypeConverter<Dictionary<string, object>, MeningoPatient>
    {
        public bool QueryCountyEnabled { get; set; } = true;

        private static readonly Dictionary<object, State> AccessStateToState = new Dictionary<object, State>
        {
            {1, State.BW},
            {2, State.BY},
            {3, State.BE},
            {4, State.BB},
            {5, State.HB},
            {6, State.HH},
            {7, State.HE},
            {8, State.MV},
            {9, State.NI},
            {10, State.NW},
            {11, State.RP},
            {12, State.SL},
            {13, State.SN},
            {14, State.ST},
            {15, State.SH},
            {16, State.TH},
            {17, State.Overseas}
        };


        private static Gender? ConvertGender(Dictionary<string, object> properties)
        {
            var accessGender = properties["geschlecht"];
            if (accessGender is DBNull || "m".Equals(accessGender) || "w".Equals(accessGender))
            {
                return accessGender is DBNull ? Gender.NotStated : "m".Equals(accessGender) ? Gender.Male : Gender.Female;
            }

            throw new Exception($"Unknown gender: '{accessGender}'");
        }

        public MeningoPatient Convert(Dictionary<string, object> source, MeningoPatient destination,
            ResolutionContext context)
        {
            var patient = CreateMeningoPatient(source);
            QueryCounty(source, patient);
            PopulateClinicalInformation(source, patient);
            return patient;
        }

        private static void PopulateClinicalInformation(Dictionary<string, object> source, MeningoPatient patient)
        {
            if (source["meningitis"].Equals(true))
            {
                patient.ClinicalInformation |= MeningoClinicalInformation.Meningitis;
            }

            if (source["sepsis"].Equals(true))
            {
                patient.ClinicalInformation |= MeningoClinicalInformation.Sepsis;
            }

            if (source["wfs"].Equals(true))
            {
                patient.ClinicalInformation |= MeningoClinicalInformation.WaterhouseFriderichsenSyndrome;
            }

            if (source["k_sympt"].Equals(true))
            {
                patient.ClinicalInformation |= MeningoClinicalInformation.NoSymptoms;
            }

            if (source["n_spez"].Equals(true))
            {
                patient.ClinicalInformation |= MeningoClinicalInformation.NotAvailable;
            }

            if (!(source["sonst_sympt"] is DBNull))
            {
                patient.ClinicalInformation |= MeningoClinicalInformation.Other;
                patient.OtherClinicalInformation = source["sonst_sympt"].ToString();
            }

            if (!(source["and_inv_erkr"] is DBNull))
            {
                patient.ClinicalInformation |= MeningoClinicalInformation.Other;
                InitializeOrAmendOtherClinicalInformation(patient);
                patient.OtherClinicalInformation += source["and_inv_erkr"].ToString();
            }

            if (!(source["grunderkr"] is DBNull))
            {
                patient.ClinicalInformation |= MeningoClinicalInformation.Other;
                InitializeOrAmendOtherClinicalInformation(patient);
                patient.OtherClinicalInformation += source["grunderkr"].ToString();
            }
        }

        private void QueryCounty(Dictionary<string, object> source, MeningoPatient patient)
        {
            if (!QueryCountyEnabled) return;
            var json = Geonames.QueryGeonames(source["plz"].ToString());
            var jObject = JObject.Parse(json);
            var postalCodes = jObject["postalcodes"];
            if (postalCodes.Any())
            {
                var city = postalCodes[0]["placeName"];
                var county = postalCodes[0]["adminName3"];
                var state = postalCodes[0]["adminCode1"];
                patient.County = county.ToString();
            }
        }

        private static MeningoPatient CreateMeningoPatient(Dictionary<string, object> source)
        {
            var patient = new MeningoPatient
            {
                PatientId = (int) source["patnr"],
                Initials = source["initialen"].ToString(),
                BirthDate = source["geb_dat"] is DBNull
                    ? (DateTime?) null
                    : DateTime.Parse(source["geb_dat"].ToString()),
                Gender = ConvertGender(source),
                PostalCode = source["plz"].ToString(),
                City = source["wohnort"].ToString(),
                State = source["bundeslandnr"] is DBNull
                    ? State.Unknown
                    : AccessStateToState[source["bundeslandnr"]]
            };
            if (string.IsNullOrEmpty(patient.PostalCode))
            {
                patient.PostalCode = "keine Angabe";
            }
            if (string.IsNullOrEmpty(patient.Initials))
            {
                patient.Initials = "?.?.";
            }
            return patient;
        }

        private static void InitializeOrAmendOtherClinicalInformation(MeningoPatient patient)
        {
            if (!string.IsNullOrEmpty(patient.OtherClinicalInformation))
            {
                patient.OtherClinicalInformation += ", ";
            }
            else
            {
                patient.OtherClinicalInformation = "";
            }
        }
    }
}