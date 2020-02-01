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

namespace HaemophilusWeb.Services
{
    public class PubMlstService
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private const string BaseUrl = "http://rest.pubmlst.org/db/pubmlst_neisseria_isolates/isolates";

        private readonly Func<string, string> callGetUrl;

        public PubMlstService() : this(CallUrlViaGet, CallUrlViaPost)
        {
        }

        public PubMlstService(Func<string, string> callGetUrl, Func<string, Dictionary<string,string>, string> callPostUrl)
        {
            this.callGetUrl = callGetUrl;
        }


        public virtual NeisseriaPubMlstIsolate GetIsolateByReference(string isolateReference)
        {
            var isolateRecord = JObject.Parse(CallUrlViaPost($"{BaseUrl}/search",
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
                var fields = isolateJson.GetValue("schemes").First["fields"];

                var allelesJson = JObject.Parse(callGetUrl($"{BaseUrl}/{id}/allele_ids?return_all=1"));
                var alleles = ConvertToDictionary(allelesJson.GetValue("allele_ids"));
                var isolate = new NeisseriaPubMlstIsolate
                {
                    PubMlstId = id,
                    PorAVr1 = alleles.Get("PorA_VR1", ""),
                    PorAVr2 = alleles.Get("PorA_VR2", ""),
                    FetAVr = alleles.Get("FetA_VR", ""),
                    PorB = alleles.Get("'porB", ""),
                    Fhbp = alleles.Get("'fHbp", ""),
                    Nhba = alleles.Get("NHBA_peptide", ""),
                    NadA = alleles.Get("NadA_peptide", ""),
                    PenA = alleles.Get("penA", ""),
                    GyrA = alleles.Get("gyrA", ""),
                    ParC = alleles.Get("NEIS1525", ""),
                    ParE = alleles.Get("NEIS1600", ""),
                    RpoB = alleles.Get("rpoB", ""),
                    RplF = alleles.Get("'rplF", ""),
                    SequenceType = fields["ST"]?.Value<string>(),
                    ClonalComplex = fields["clonal_complex"]?.Value<string>()
                };
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

        private static string CallUrlViaGet(string url)
        {
            using (var client = new HttpClient())
            using (var result = client.GetAsync(new Uri(url)).Result)
            {
                if (result.StatusCode == HttpStatusCode.NotFound)
                {
                    throw new WebException();
                }
                return result.Content.ReadAsStringAsync().Result;
            }
        }

        private static string CallUrlViaPost(string url, Dictionary<string,string> parameters)
        {
            var content = new StringContent(JsonSerializer.SerializeToString(parameters));
            using (var client = new HttpClient())
            using (var result = client.PostAsync(new Uri(url), content).Result)
            {
                if (result.StatusCode == HttpStatusCode.NotFound)
                {
                    throw new WebException();
                }
                return result.Content.ReadAsStringAsync().Result;
            }
        }
    }
}