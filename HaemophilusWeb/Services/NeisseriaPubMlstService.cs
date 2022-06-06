using System;
using System.Collections.Generic;
using DocumentFormat.OpenXml.Drawing.Charts;
using static HaemophilusWeb.Utils.HttpClientWrapper;

namespace HaemophilusWeb.Services 
{
    public class NeisseriaPubMlstService : PubMlstService
    {
        protected override string Database => "pubmlst_neisseria_isolates";
        
        public NeisseriaPubMlstService() : this(CallUrlViaGet, CallUrlViaPost)
        {
        }

        public NeisseriaPubMlstService(Func<string, string> callGetUrl, Func<string, Dictionary<string,string>, string> callPostUrl) : base(callGetUrl, callPostUrl)
        {
        }

        protected override string GetSearchUrl(string isolateReference)
        {
            return $"{IsolatesUrl}/search";
        }

        protected override string GetIdUrl(int id)
        {
            return $"{IsolatesUrl}/{id}";
        }

        protected override string GetAlleleIdUrl(int id)
        {
            return $"{IsolatesUrl}/{id}/allele_ids?return_all=1";
        }
    }
}