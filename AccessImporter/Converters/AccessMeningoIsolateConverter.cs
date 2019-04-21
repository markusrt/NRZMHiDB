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
    public class AccessMeningoIsolateConverter : ITypeConverter<Dictionary<string, object>, MeningoIsolate>
    {
        public MeningoIsolate Convert(Dictionary<string, object> source, MeningoIsolate destination,
            ResolutionContext context)
        {
            var isolate = CreateMeningoIsolate(source);
            FillPorAPcr(isolate, source);
            FillFetAPcr(isolate, source);
            FillPubMlst(isolate, source);
            return isolate;
        }


        private static MeningoIsolate CreateMeningoIsolate(Dictionary<string, object> source)
        {
            //"Patienten.patnr", "", "penicillin", "cefotaxim", "ciprofloxacin", "rifampicin", "", "",
            // "Serotyp", "", "", "", "st", "",
            //"cc", "pena", "fHbp"

            var stemNumber = int.TryParse(SanitizeStemnumber(source), out int stemNumberInt) ? stemNumberInt : (int?) null;
            object serogruppe = source["serogruppe"];
            var isolate = new MeningoIsolate
            {
                StemNumber = stemNumber,
                RplF = StringOrNull(source, "rplF"),
                Agglutination = AgglutinationMap[serogruppe.ToString()],
                RibosomalRna16S = RibosomalRna16SMap[source["univ_pcr"].ToString()],
                RibosomalRna16SBestMatch = StringOrNull(source, "sequenz"),
                Remark = StringOrNull(source, "Staemme.notizen")
            };
            if ("h".Equals(source["art"]))
            {
                isolate.GrowthOnBloodAgar = Growth.No;
                isolate.GrowthOnMartinLewisAgar = Growth.No;
            }
            return isolate;
        }

        private static string SanitizeStemnumber(Dictionary<string, object> source)
        {
            var stemNumber = source["stammnr"].ToString();
            if (stemNumber.Contains("MZ"))
            {
                stemNumber = string.Empty;
            }
            else if (stemNumber.Contains("/"))
            {
                stemNumber = stemNumber.Split('/').First();
            }
            else if("E6".Equals(stemNumber) || stemNumber.ToLower().StartsWith("epsilon"))
            {
                var remark = StringOrNull(source, "Staemme.notizen");
                var newRemark = string.Empty;
                if (remark != null)
                {
                    newRemark = remark + ", ";
                }
                newRemark += $"Stammnummer studie: {stemNumber}";
                source["Staemme.notizen"] = newRemark;
            }
            return stemNumber;
        }

        private static string StringOrNull(Dictionary<string, object> source, string property)
        {
            return string.IsNullOrEmpty(source[property].ToString()) ? null : source[property].ToString();
        }

        private static void FillPorAPcr(MeningoIsolate isolate, Dictionary<string, object> source)
        {
            var vr1 = source["vr1"].ToString();
            var vr2 = source["vr2"].ToString();
            if (!string.IsNullOrEmpty(vr1))
            {
                isolate.PorAVr1 = vr1;
                isolate.PorAPcr = NativeMaterialTestResult.Positive;
            }
            if (!string.IsNullOrEmpty(vr2))
            {
                isolate.PorAVr2 = vr2;
                isolate.PorAPcr = NativeMaterialTestResult.Positive;
            }
        }

        private static void FillFetAPcr(MeningoIsolate isolate, Dictionary<string, object> source)
        {
            var fetAVr = source["fet-a"].ToString();
            switch (fetAVr)
            {
                case "del.":
                case "-":
                    isolate.FetAPcr = NativeMaterialTestResult.Negative;
                    break;
                case "inhib.":
                    isolate.FetAPcr = NativeMaterialTestResult.Inhibitory;
                    break;
                case var _ when !string.IsNullOrEmpty(fetAVr):
                    isolate.FetAPcr =NativeMaterialTestResult.Positive;
                    isolate.FetAVr = fetAVr;
                    break;
            }
        }


        private void FillPubMlst(MeningoIsolate isolate, Dictionary<string, object> source)
        {
            var porB = StringOrNull(source, "Serotyp");
            var sequenceType = StringOrNull(source, "st");
            var clonalComplex = StringOrNull(source, "cc");
            var penA = StringOrNull(source, "pena");
            var fhbp = StringOrNull(source, "fHbp");

            if (!string.IsNullOrEmpty($"{porB}{sequenceType}{clonalComplex}{penA}{fhbp}"))
            {
                isolate.PubMlstIsolate = new NeisseriaPubMlstIsolate
                {
                    PorB = porB,
                    SequenceType = sequenceType,
                    ClonalComplex = clonalComplex,
                    PenA = penA,
                    Fhbp = fhbp
                };
            }
        }

        private static Dictionary<string, NativeMaterialTestResult> RibosomalRna16SMap = new Dictionary<string, NativeMaterialTestResult>
        {
            {string.Empty, NativeMaterialTestResult.NotDetermined},
            {"n", NativeMaterialTestResult.Negative},
            {"i", NativeMaterialTestResult.Inhibitory},
            {"p", NativeMaterialTestResult.Positive}
        };

        private static Dictionary<object, MeningoSerogroupAgg> AgglutinationMap = new Dictionary<object, MeningoSerogroupAgg>
        {
            {string.Empty, MeningoSerogroupAgg.NotDetermined},
            {"A", MeningoSerogroupAgg.A},
            {"auto", MeningoSerogroupAgg.Auto},
            {"B", MeningoSerogroupAgg.B},
            {"C", MeningoSerogroupAgg.C},
            {"E", MeningoSerogroupAgg.E},
            {"neagtiv", MeningoSerogroupAgg.Negative},
            {"poly", MeningoSerogroupAgg.Poly},
            {"W", MeningoSerogroupAgg.W},
            {"X", MeningoSerogroupAgg.X},
            {"Y", MeningoSerogroupAgg.Y},
            {"Z", MeningoSerogroupAgg.Z},
            {"NG", MeningoSerogroupAgg.Negative},
            {"ng", MeningoSerogroupAgg.Negative},
            {"cnl", MeningoSerogroupAgg.Cnl},
            {"Alle", MeningoSerogroupAgg.Poly},
            {"NSG", MeningoSerogroupAgg.Negative},
            {"inhibitorisch", MeningoSerogroupAgg.NotDetermined},
        };
    }
}