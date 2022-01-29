using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using Geolocation;
using HaemophilusWeb.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using static HaemophilusWeb.Utils.HttpClientWrapper;

namespace HaemophilusWeb.Utils
{
    public class GeonamesService : IGeonamesService
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private const string BaseUrl = "http://api.geonames.org";
        private readonly Func<string, string> callGetUrl;
        private string geonamesAccount;

        public GeonamesService() : this(CallUrlViaGet)
        {
        }

        public GeonamesService(Func<string, string> callGetUrl)
        {
            this.geonamesAccount = ConfigurationManager.AppSettings["GeonamesAccount"];
            this.callGetUrl = callGetUrl;
        }

        public virtual string QueryByPostalCode(string postalCode, string placeName = "")
        {
            try
            {
                var queryResult = JObject.Parse(callGetUrl($"{BaseUrl}/postalCodeLookupJSON?country=DE&username={geonamesAccount}&postalcode={postalCode}"));
                var postalCodes = queryResult["postalcodes"] as JArray ?? new JArray();
                var filteredPostalCodes = new JArray();

                if (!string.IsNullOrEmpty(placeName))
                {
                    var matchingPlace = postalCodes.FirstOrDefault(p => placeName.Equals(p.Value<string>("placeName")));
                    if (matchingPlace != null)
                    {
                        filteredPostalCodes.Add(matchingPlace);
                    }
                }
                else if (postalCodes.Any())
                {
                    filteredPostalCodes.Add(postalCodes.First());
                }

                queryResult["postalcodes"] = filteredPostalCodes;
                return JsonConvert.SerializeObject(queryResult, Formatting.None);
            }
            catch (WebException e)
            {
                Log.Error(e, "$Failed to call Geonames PostalCodeLookupJSON REST API.");
                return "{\"postalcodes\":[]}";
            }
        }

        public string QueryByPostalCodePrefix(string postalCodePrefix)
        {
            try
            {
                var queryResult = JObject.Parse(callGetUrl($"{BaseUrl}/postalCodeSearchJSON?country=DE&username={geonamesAccount}&postalcode_startsWith={postalCodePrefix}"));
                return JsonConvert.SerializeObject(queryResult, Formatting.None);
            }
            catch (WebException e)
            {
                Log.Error(e, "$Failed to call Geonames PostalCodeSearchJSON REST API.");
                return "{\"postalcodes\":[]}";
            }
        }

        public Coordinate? QueryCoordinateByPostalCode(string postalCode, string placeName = "")
        {
            var queryResult = JObject.Parse(QueryByPostalCode(postalCode, placeName))["postalcodes"] as JArray;
            if (queryResult.Count > 0)
            {
                return new Coordinate(queryResult.First.Value<double>("lat"), queryResult.First.Value<double>("lng"));
            }

            return null;
        }
    }
}