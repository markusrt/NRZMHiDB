using System;
using System.IO;
using System.Net;
using System.Web.Mvc;
using DataTables.Mvc;
using HaemophilusWeb.Models;

namespace HaemophilusWeb.Controllers
{
    public class GeonamesController : Controller
    {
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
            using (var client = new WebClient())
            using (var result = client.OpenRead(new Uri("http://api.geonames.org/postalCodeLookupJSON?country=DE&username=hweb&postalcode=" + postalCode)))
            using (var reader = new StreamReader(result ?? new MemoryStream()))
            {
                return new ContentResult { Content = reader.ReadToEnd(), ContentType = "application/json" };
            }
        }
    }
}