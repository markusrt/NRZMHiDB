using System;
using System.Collections.Generic;

namespace HaemophilusWeb.Services;

using static HaemophilusWeb.Utils.HttpClientWrapper;

public class NeisseriaPubMlstService : PubMlstService
{
    protected override string BaseUrl => "http://rest.pubmlst.org/db/pubmlst_neisseria_isolates/isolates";

    public NeisseriaPubMlstService() : this(CallUrlViaGet, CallUrlViaPost)
    {
    }

    public NeisseriaPubMlstService(Func<string, string> callGetUrl, Func<string, Dictionary<string,string>, string> callPostUrl) : base(callGetUrl, callPostUrl)
    {
    }

    protected override string GetSearchUrl(string isolateReference)
    {
        return $"{BaseUrl}/search";
    }

    protected override string GetIdUrl(int id)
    {
        return $"{BaseUrl}/{id}";
    }

    protected override string GetAlleleIdUrl(int id)
    {
        return $"{BaseUrl}/{id}/allele_ids?return_all=1";
    }
}