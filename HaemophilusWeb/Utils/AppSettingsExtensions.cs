using System.Collections.Specialized;
using HaemophilusWeb.Services;

namespace HaemophilusWeb.Utils;

public static class AppSettingsExtensions
{
    public static PubMlstAuthorization GetIrisAuthentication(this NameValueCollection appSettings)
    {
        return new PubMlstAuthorization
        {
            AccessToken = appSettings["IrisAccessToken"],
            AccessTokenSecret = appSettings["IrisAccessTokenSecret"],
            ConsumerKey = appSettings["IrisConsumerKey"],
            ConsumerSecret = appSettings["IrisConsumerSecret"]
        };
    }
}