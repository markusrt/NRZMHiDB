using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Web.Helpers;
using HaemophilusWeb.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HaemophilusWeb.Tools
{
    public class RkiTool
    {
        private const string YqlUri = "https://query.yahooapis.com/v1/public/yql?q=select%20*%20from%20html%20where%20url%3D%22http%3A%2F%2Ftools.rki.de%2FPLZTool%2F%3Fq%3D{0}%22%20and%20xpath%3D%22%2F%2Finput%5B%40type%3D%27text%27%5D%20%7C%20%2F%2Faddress%2Fdescendant-or-self%3A%3A*%2Ftext()%5Bnormalize-space()%5D%22&format=json";
        private readonly Func<string, string> queryPostalCode;

        public RkiTool() : this(QueryUsingWebClient)
        {
        }

        private static string QueryUsingWebClient(string postalCode)
        {
            using (var client = new WebClient())
            using (var result = client.OpenRead(new Uri(string.Format(YqlUri, postalCode))))
            using (var reader = new StreamReader(result ?? new MemoryStream()))
            {
                return reader.ReadToEnd();
            }
        }

        public RkiTool(Func<string, string> queryPostalCode )
        {
            this.queryPostalCode = queryPostalCode;
        }

        public HealthOffice QueryHealthOffice(string postalCode)
        {
            try
            {
                var result = queryPostalCode(postalCode);
                JToken jobject = JObject.Parse(result);
                var results = jobject["query"]["results"];

                if (!results.HasValues)
                {
                    return null;
                }
                var inputs = results["input"];
                var address = Regex.Replace(results["content"].ToString(), "\r\n[ \t]*", "\n");
                return new HealthOffice
                {
                    Address = address,
                    Phone = inputs[0]["value"].ToString(),
                    Fax = inputs[1]["value"].ToString(),
                    Email = inputs[2]["value"].ToString(),
                    PostalCode = postalCode
                };
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}