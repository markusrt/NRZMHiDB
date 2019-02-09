using System;
using System.IO;
using System.Net;

namespace HaemophilusWeb.Utils
{
    public static class Geonames
    {
        public static string QueryGeonames(string postalCode)
        {
            using (var client = new WebClient())
            using (var result =
                client.OpenRead(new Uri("http://api.geonames.org/postalCodeLookupJSON?country=DE&username=hweb&postalcode=" +
                                        postalCode)))
            using (var reader = new StreamReader(result ?? new MemoryStream()))
            {
                return reader.ReadToEnd();
            }
        }
    }
}