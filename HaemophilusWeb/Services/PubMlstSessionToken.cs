using Newtonsoft.Json;

namespace HaemophilusWeb.Services;

public class PubMlstSessionToken
{
    [JsonProperty(PropertyName = "oauth_token")]
    public string Token { get; set; }

    [JsonProperty(PropertyName = "oauth_token_secret")]
    public string TokenSecret { get; set; }
}