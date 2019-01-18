using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using HaemophilusWeb.Models;
using Newtonsoft.Json.Linq;
using NLog;
using ServiceStack;
using ServiceStack.Text;

namespace HaemophilusWeb.Controllers
{
    public class PubMlstController : Controller
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private readonly Func<string, string> _callRemoteUrl;

        public PubMlstController() : this(CallUrlViaWebClient)
        {

        }

        public PubMlstController(Func<string,string> callRemoteUrl )
        {
            _callRemoteUrl = callRemoteUrl;
        }

        [HttpPost]
        [Authorize(Roles = DefaultRoles.User)]
        public ActionResult NeisseriaIsolates(int isolateId)
        {
            var isolate = new NeisseriaPubMlstIsolate();

            try
            {
                var getAlleles =
                    $"http://rest.pubmlst.org/db/pubmlst_neisseria_isolates/isolates/{isolateId}/allele_ids?return_all=1";
                var json = JObject.Parse(_callRemoteUrl(getAlleles));
                var alleles = ConvertToDictionary(json.GetValue("allele_ids"));

                isolate.PorAVr1 = alleles.Get("PorA_VR1", "");
                isolate.PorAVr2 = alleles.Get("PorA_VR2", "");
                isolate.FetAVr = alleles.Get("FetA_VR", "");
                isolate.PorB = alleles.Get("'porB", "");
                isolate.Fhbp = alleles.Get("'fHbp", "");
                isolate.Nhba = alleles.Get("NHBA_peptide", "");
                isolate.NadA = alleles.Get("NadA_peptide", "");
                isolate.PenA = alleles.Get("penA", "");
                isolate.GyrA = alleles.Get("gyrA", "");
                isolate.ParC = alleles.Get("NEIS1525", "");
                isolate.ParE = alleles.Get("NEIS1600", "");
                isolate.RpoB = alleles.Get("rpoB", "");
                isolate.RplF = alleles.Get("'rplF", "");
                isolate.SequenceType = "TBD";
                isolate.ClonalComplex = "TBD";
            }
            catch (WebException e)
            {
                if (e.IsNotFound())
                {
                    return new HttpNotFoundResult();
                }
                Log.Error(e, "$Failed to call PubMLST REST API.");
            }
            return Json(isolate);
        }

        private Dictionary<string,string> ConvertToDictionary(JToken alleleIds)
        {
            var dictionary = new Dictionary<string,string>();
            if (alleleIds.HasValues)
            {
                JToken current = alleleIds.First;
                do
                {
                    dynamic children = current.Children().Single();
                    dictionary.Add(children.Name, children.Value.ToString());
                } while ((current = current.Next) != null);
            }
            return dictionary;
        }

        private static string CallUrlViaWebClient(string url)
        {
            using (var client = new WebClient())
            using (var result = client.OpenRead(new Uri(url)))
            using (var reader = new StreamReader(result ?? new MemoryStream()))
            {
                return reader.ReadToEnd();
            }
        }
    }
}