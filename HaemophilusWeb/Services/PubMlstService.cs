using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using HaemophilusWeb.Models;
using Newtonsoft.Json.Linq;
using NLog;
using ServiceStack.Text;

namespace HaemophilusWeb.Services
{
    public abstract class PubMlstService
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

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
        public const string BexseroReactivity = "Bexsero_reactivity";
        public const string TrumenbaReactivity = "Trumenba_reactivity";

        protected Func<string, string> CallGetUrl { get; }
        protected Func<string, Dictionary<string, string>, string> CallPostUrl { get; }

        protected abstract string BaseUrl { get; }

        protected abstract string GetSearchUrl(string isolateReference);
        
        protected abstract string GetIdUrl(int id);
        
        protected abstract string GetAlleleIdUrl(int id);

        protected PubMlstService(Func<string, string> callGetUrl, Func<string, Dictionary<string,string>, string> callPostUrl)
        {
            CallGetUrl = callGetUrl;
            CallPostUrl = callPostUrl;
        }
        
        public virtual NeisseriaPubMlstIsolate GetIsolateByReference(string isolateReference)
        {
            var searchUrl = GetSearchUrl(isolateReference);
            var isolateRecord = JObject.Parse(CallPostUrl(searchUrl,
                    new Dictionary<string, string> {{"field.isolate", isolateReference}}));

            if (isolateRecord["records"].Value<string>() != "1")
            {
                Log.Info($"PubMLST query <{searchUrl}> returned either no or more than one result: {isolateRecord}");
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
                var isolateJson = JObject.Parse(CallGetUrl(GetIdUrl(id)));

                var allelesJson = JObject.Parse(CallGetUrl(GetAlleleIdUrl(id)));
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

                var fields = isolateJson.GetValue("schemes")?.First["fields"];
                if (fields != null)
                {
                    isolate.SequenceType = fields[SequenceType]?.Value<string>();
                    isolate.ClonalComplex = fields[ClonalComplex]?.Value<string>();
                }

                var phenotypic = isolateJson["phenotypic"];
                if (phenotypic != null)
                {
                    isolate.BexseroReactivity = phenotypic.Value<string>(BexseroReactivity);
                    isolate.TrumenbaReactivity = phenotypic.Value<string>(TrumenbaReactivity);
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