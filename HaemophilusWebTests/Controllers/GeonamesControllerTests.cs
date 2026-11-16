using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using FluentAssertions;
using HaemophilusWeb.Models;
using HaemophilusWeb.Models.Meningo;
using HaemophilusWeb.Services;
using HaemophilusWeb.Utils;
using NUnit.Framework;

namespace HaemophilusWeb.Controllers
{
    public class GeonamesControllerTests
    {
        [Test]
        public void LoadCountries_DeserializesJson()
        {
            var sut = new GeonamesController(new GeonamesService(), LoadMockCountries);

            var countries = sut.LoadCountries();

            countries.Should().HaveCount(1);
            countries.First().Capital.Should().Be("Kabul");
        }

        // specs/022 FR-010, US3.1, US3.3: the country list is read from the stored country file and ordered by the
        // German name (culture-aware, as on the host: "Ägypten" follows "Afghanistan")
        [Test]
        public void LoadCountries_ReadsCountryFileAndOrdersByName()
        {
            string requestedPath = null;
            var sut = new GeonamesController(new GeonamesService(), path =>
            {
                requestedPath = path;
                return "{\"geonames\":[" +
                       "{\"isoAlpha3\":\"CYP\",\"countryName\":\"Zypern\"}," +
                       "{\"isoAlpha3\":\"DEU\",\"countryName\":\"Deutschland\"}," +
                       "{\"isoAlpha3\":\"EGY\",\"countryName\":\"Ägypten\"}," +
                       "{\"isoAlpha3\":\"AFG\",\"countryName\":\"Afghanistan\"}]}";
            });
            var previousCulture = CultureInfo.CurrentCulture;
            List<Country> countries;
            try
            {
                CultureInfo.CurrentCulture = new CultureInfo("de-DE");
                countries = sut.LoadCountries();
            }
            finally
            {
                CultureInfo.CurrentCulture = previousCulture;
            }

            requestedPath.Should().Be("~/App_Data/countries.json");
            countries.Select(c => c.CountryName).Should().Equal("Afghanistan", "Ägypten", "Deutschland", "Zypern");
            countries.Select(c => c.IsoAlpha3).Should().Equal("AFG", "EGY", "DEU", "CYP");
        }

        // specs/022 FR-010, US3.2, US3.5: a patient without stored country (both pathogens) has "DEU"
        [Test]
        public void PatientWithoutCountry_DefaultsToDeu()
        {
            var patient = new Patient();
            var meningoPatient = new MeningoPatient { Country = null };

            patient.Country.Should().Be(GeonamesController.DefaultCountryIsoAlpha3).And.Be("DEU");
            meningoPatient.Country.Should().Be("DEU");
        }

        private string LoadMockCountries(string serverPath)
        {
            return
                "{\"geonames\":[{\"continent\":\"AS\",\"capital\":\"Kabul\",\"languages\":\"fa-AF,ps,uz-AF,tk\",\"geonameId\":1149361,\"south\":29.3770645357176,\"isoAlpha3\":\"AFG\",\"north\":38.4907920755748,\"fipsCode\":\"AF\",\"population\":\"29121286\",\"east\":74.8894511481168,\"isoNumeric\":\"004\",\"areaInSqKm\":\"647500.0\",\"countryCode\":\"AF\",\"west\":60.4720833972263,\"countryName\":\"Afghanistan\",\"continentName\":\"Asien\",\"currencyCode\":\"AFN\"}]}";
        }
    }
}