using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using AutoMapper;
using HaemophilusWeb.Models;
using HaemophilusWeb.Models.Meningo;
using HaemophilusWeb.Utils;
using Newtonsoft.Json.Linq;

namespace AccessImporter.Converters
{
    public class AccessMeningoSendingConverter : ITypeConverter<Dictionary<string, object>, MeningoSending>
    {
        public MeningoSending Convert(Dictionary<string, object> source, MeningoSending destination,
            ResolutionContext context)
        {
            var sending = CreateMeningoSending(source);
            FillSamplingLocation(sending, source);
            return sending;
        }

        private void FillSamplingLocation(MeningoSending sending, Dictionary<string, object> source)
        {
            var materialNr = source["isol_mat_nr"];
            sending.SamplingLocation = Mapper.Map<MeningoSamplingLocation>(materialNr);

            if (20.Equals(materialNr))
            {
                sending.OtherInvasiveSamplingLocation = "nicht-steriles Mat. bei IMD";
            }
        }

        private static MeningoSending CreateMeningoSending(Dictionary<string, object> source)
        {
            var sending = new MeningoSending
            {
                MeningoPatientId = (int) source["Patienten.patnr"],
                LaboratoryNumber = SanitizeLaboratoryNumber(source["labornr"].ToString()),
                Material = ConvertToMaterial(source["art"]),
                SenderLaboratoryNumber = source["nr_eins"].ToString(),
                ReceivingDate = String.IsNullOrEmpty(source["eing_dat"].ToString())
                    ? DateTime.MinValue : DateTime.Parse(source["eing_dat"].ToString()),
                SamplingDate = source["entn_dat"] is DBNull
                    ? (DateTime?)null : DateTime.Parse(source["entn_dat"].ToString()),
                SerogroupSender = source["erg_eins"].ToString(),
                Remark = source["Patienten.notizen"].ToString()
            };

            return sending;
        }

        private static MeningoMaterial ConvertToMaterial(object material)
        {
            if ("v".Equals(material))
            {
                return MeningoMaterial.VitalStem;
            }
            if ("h".Equals(material))
            {
                //TODO return MeningoMaterial.HeatActivated ;
                return MeningoMaterial.NoGrowth;
            }
            if ("d".Equals(material))
            {
                return MeningoMaterial.IsolatedDna;
            }
            if ("n".Equals(material))
            {
                return MeningoMaterial.NativeMaterial;
            }
            if ("t".Equals(material))
            {
                return MeningoMaterial.NoGrowth;
            }
            throw new ArgumentException($"Unknown material: {material}");
        }

        private static Dictionary<string, string> LaboratoryNumberFixes = new Dictionary<string, string>
        {
            {"M/172", "MZ172"},
            {"MZ 197", "MZ197"},
            {"MZ 449", "MZ449"},
            {"MZ 505", "MZ505"},
            {"MZ 554", "MZ554"},
            {"MZ/06", "MZ6"}
        };

        private static string SanitizeLaboratoryNumber(string laboratoryNumber)
        {
            if (LaboratoryNumberFixes.ContainsKey(laboratoryNumber))
            {
                laboratoryNumber = LaboratoryNumberFixes[laboratoryNumber];
            }
            if (Regex.IsMatch(laboratoryNumber, "^MZ\\d+$"))
            {
                laboratoryNumber = laboratoryNumber + "/02";
            }
            return laboratoryNumber.Replace("MZ", "");
        }
    }
}