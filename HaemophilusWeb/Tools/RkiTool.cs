using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Web.Helpers;
using HaemophilusWeb.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HaemophilusWeb.Tools
{
    public class RkiTool
    {
        private const string YqlUri = "https://query.yahooapis.com/v1/public/yql?q=select%20*%20from%20htmlpost%20where%0Aurl%3D%27https%3A%2F%2Ftools.rki.de%2FPLZTool%2Fde-DE%2FHome%2FSearch%27%20%0Aand%20postdata%3D%22RequestString%3D{0}%22%20and%20xpath%3D%22%2F%2Fdiv%5B%40class%3D%27tab-row%27%5D%22&env=store%3A%2F%2Fdatatables.org%2Falltableswithkeys&format=json";
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
                var postresult = jobject["query"]["results"]["postresult"];

                if (!postresult.HasValues)
                {
                    return null;
                }
                var divs = postresult["div"];
                var address = divs[0]["textarea"]["content"].ToString();
                return new HealthOffice
                {
                    Address = address.Replace("\r\n", "\n"),
                    Phone = divs[1]["textarea"]["content"].ToString(),
                    Fax = divs[2]["textarea"]["content"].ToString(),
                    Email = divs[3]["a"]["content"].ToString(),
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