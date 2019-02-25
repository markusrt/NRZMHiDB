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
            return isolate;
        }
        
        private static MeningoIsolate CreateMeningoIsolate(Dictionary<string, object> source)
        {
            //"Patienten.patnr", "", "penicillin", "cefotaxim", "ciprofloxacin", "rifampicin", "rplF", "",
            // "Serotyp", "", "", "", "st", "fet-a",
            //"cc", "pena", "fHbp"
            var stemNumber = int.TryParse(source["stammnr"].ToString(), out int stemNumberInt) ? stemNumberInt : (int?) null;
            object serogruppe = source["serogruppe"];
            var isolate = new MeningoIsolate
            {
                StemNumber = stemNumber,
                RplF = source["rplF"].ToString(),
                Agglutination = AgglutinationMap[serogruppe.ToString()],
                RibosomalRna16S = RibosomalRna16SMap[source["univ_pcr"].ToString()],
                RibosomalRna16SBestMatch = string.IsNullOrEmpty(source["sequenz"].ToString()) ? null : source["sequenz"].ToString(),
                Remark = string.IsNullOrEmpty(source["Staemme.notizen"].ToString()) ? null : source["Staemme.notizen"].ToString(),
            };
            return isolate;
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
            {"NG", MeningoSerogroupAgg.NotDetermined},
            {"ng", MeningoSerogroupAgg.NotDetermined},
            {"cnl", MeningoSerogroupAgg.NotDetermined},
            {"Alle", MeningoSerogroupAgg.NotDetermined},
            {"NSG", MeningoSerogroupAgg.NotDetermined},
            {"inhibitorisch", MeningoSerogroupAgg.NotDetermined},
        };
    }
}