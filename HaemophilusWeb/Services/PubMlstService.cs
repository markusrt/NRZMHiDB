using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Mvc;
using HaemophilusWeb.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using NLog.Fluent;
using ServiceStack.Text;
using JsonSerializer = ServiceStack.Text.JsonSerializer;
using static HaemophilusWeb.Utils.HttpClientWrapper;

namespace HaemophilusWeb.Services
{
    public class PubMlstService
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private const string BaseUrl = "http://rest.pubmlst.org/db/pubmlst_neisseria_isolates/isolates";
        public const string PorAVr1 = "PorA_VR1";
        public const string PorAVr2 = "PorA_VR2";
        public const string FetAVr = "FetA_VR";
        public const string PorB = "'porB";
        public const string NhbaPeptide = "NHBA_peptide";
        public const string NadaPeptide = "NadA_peptide";
        public const string PenA = "penA";
        public const string GyrA = "gyrA";
        public const string Neis1525 = "NEIS1525";
        public const string RpoB = "rpoB";
        public const string RplF = "'rplF";
        public const string Fhbp = "'fHbp";
        public const string Neis1600 = "NEIS1600";
        public const string SequenceType = "ST";
        public const string ClonalComplex = "clonal_complex";

        private readonly Func<string, string> callGetUrl;
        private readonly Func<string, Dictionary<string, string>, string> callPostUrl;

        public PubMlstService() : this(CallUrlViaGet, CallUrlViaPost)
        {
        }

        public PubMlstService(Func<string, string> callGetUrl, Func<string, Dictionary<string,string>, string> callPostUrl)
        {
            this.callGetUrl = callGetUrl;
            this.callPostUrl = callPostUrl;
        }


        public virtual NeisseriaPubMlstIsolate GetIsolateByReference(string isolateReference)
        {
            var isolateRecord = JObject.Parse(callPostUrl($"{BaseUrl}/search",
                    new Dictionary<string, string> {{"field.isolate", isolateReference}}));

            if (isolateRecord["records"].Value<string>() != "1")
            {
                Log.Error($"Failed to find isolate by reference {isolateReference}");
                Log.Info($"PubMLST query returned either no or more than one result: {isolateRecord}");
                return null;
            }

            var isolateUri = isolateRecord["isolates"].First.Value<string>();
            var isolateId = isolateUri.Substring(BaseUrl.Length + 1);
            return GetIsolateById(int.Parse(isolateId));
        }

        public virtual NeisseriaPubMlstIsolate GetIsolateById(int id)
        {
            try
            {
                var isolateJson = JObject.Parse(callGetUrl($"{BaseUrl}/{id}"));

                var allelesJson = JObject.Parse(callGetUrl($"{BaseUrl}/{id}/allele_ids?return_all=1"));
                var alleles = ConvertToDictionary(allelesJson.GetValue("allele_ids"));
                var isolate = new NeisseriaPubMlstIsolate
                {
                    PubMlstId = id,
                    PorAVr1 = alleles.Get(PorAVr1, ""),
                    PorAVr2 = alleles.Get(PorAVr2, ""),
                    FetAVr = alleles.Get(FetAVr, ""),
                    PorB = alleles.Get(PorB, ""),
                    Fhbp = alleles.Get(Fhbp, ""),
                    Nhba = alleles.Get(NhbaPeptide, ""),
                    NadA = alleles.Get(NadaPeptide, ""),
                    PenA = alleles.Get(PenA, ""),
                    GyrA = alleles.Get(GyrA, ""),
                    ParC = alleles.Get(Neis1525, ""),
                    ParE = alleles.Get(Neis1600, ""),
                    RpoB = alleles.Get(RpoB, ""),
                    RplF = alleles.Get(RplF, ""),
                };

                var fields = isolateJson.GetValue("schemes").First["fields"];
                if (fields != null)
                {
                    isolate.SequenceType = fields[SequenceType]?.Value<string>();
                    isolate.ClonalComplex = fields[ClonalComplex]?.Value<string>();
                }
                return isolate;
            }
            catch (WebException e)
            {
                Log.Error(e, "$Failed to call PubMLST REST API.");
                return null;
            }
        }

        private Dictionary<string, string> ConvertToDictionary(JToken alleleIds)
        {
            var dictionary = new Dictionary<string, string>();
            if (alleleIds.HasValues)
            {
                var current = alleleIds.First;
                do
                {
                    dynamic children = current.Children().Single();
                    dictionary.Add(children.Name, children.Value.ToString());
                } while ((current = current.Next) != null);
            }
            return dictionary;
        }
    }
}