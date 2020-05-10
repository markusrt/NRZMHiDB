using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using ServiceStack.Text;

namespace HaemophilusWeb.Utils
{
    //TODO temporary solution only until switching to .net core
    public static class HttpClientWrapper
    {
        public static string CallUrlViaGet(string url)
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

        public static string CallUrlViaPost(string url, Dictionary<string, string> parameters)
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