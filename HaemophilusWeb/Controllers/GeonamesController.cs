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
using HaemophilusWeb.Services;
using HaemophilusWeb.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HaemophilusWeb.Controllers
{
    public class GeonamesController : Controller
    {
        public const string DefaultCountryIsoAlpha3 = "DEU";
        private readonly GeonamesService _geonamesService;
        private readonly Func<string, string> readServerFile;

        public GeonamesController() : this(new GeonamesService(), s => System.IO.File.ReadAllText(HostingEnvironment.MapPath(s)))
        {
        }

        public GeonamesController(GeonamesService geonamesService, Func<string,string> readServerFile)
        {
            _geonamesService = geonamesService;
            this.readServerFile = readServerFile;
        }

        [HttpPost]
        [Authorize(Roles = DefaultRoles.User)]
        public ContentResult PostalCodeStartsWith(string postalCodePrefix)
        {
            var json = _geonamesService.QueryByPostalCodePrefix(postalCodePrefix);
            return new ContentResult { Content = json, ContentType = "application/json" };
        }

        [HttpPost]
        [Authorize(Roles = DefaultRoles.User)]
        public ContentResult PostalCode(string postalCode, string placeName = "")
        {
            var json = _geonamesService.QueryByPostalCode(postalCode, placeName);
            return new ContentResult { Content = json, ContentType = "application/json" };
        }

        public List<Country> LoadCountries()
        {
            //TODO Update JSON file by querying http://api.geonames.org/countryInfoJSON from time to time
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