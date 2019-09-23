using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Hosting;
using System.Web.Mvc;
using DataTables.Mvc;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using HaemophilusWeb.Models;
using HaemophilusWeb.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HaemophilusWeb.Controllers
{
    public class GeonamesController : Controller
    {
        public const string DefaultCountryIsoAlpha3 = "DEU";
        private readonly Func<string, string> readServerFile;

        public GeonamesController()
        {
            readServerFile = s => System.IO.File.ReadAllText(HostingEnvironment.MapPath(s));
        }

        public GeonamesController(Func<string,string> readServerFile)
        {
            this.readServerFile = readServerFile;
        }

        [HttpPost]
        [Authorize(Roles = DefaultRoles.User)]
        public ContentResult PostalCodeStartsWith(string postalCodePrefix)
        {
            using (var client = new WebClient())
            using (var result = client.OpenRead(new Uri("http://api.geonames.org/postalCodeSearchJSON?postalcode_startsWith=" + postalCodePrefix + "&country=DE&username=hweb&format=json")))
            using (var reader = new StreamReader(result ?? new MemoryStream()))
            {
                return new ContentResult { Content = reader.ReadToEnd(), ContentType = "application/json" };
            }
        }

        [HttpPost]
        [Authorize(Roles = DefaultRoles.User)]
        public ContentResult PostalCode(string postalCode)
        {
            var json = Geonames.QueryGeonames(postalCode);
            return new ContentResult { Content = json, ContentType = "application/json" };
        }

        public List<Country> LoadCountries()
        {
            //TODO Update JSON file by querying http://api.geonames.org/countryInfoJSON?username=hweb&lang=de from time to time
            var countryList = readServerFile("~/App_Data/countries.json");
            var countries = JsonConvert.DeserializeObject<GeonameCountries>(countryList).Geonames;
            return countries.OrderBy(c => c.CountryName).ToList();
        }

        private class GeonameCountries
        {
            public List<Country> Geonames { get; set; }
        }
    }
}