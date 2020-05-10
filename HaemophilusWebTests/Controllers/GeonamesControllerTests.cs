using System.Linq;
using FluentAssertions;
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

        private string LoadMockCountries(string serverPath)
        {
            return
                "{\"geonames\":[{\"continent\":\"AS\",\"capital\":\"Kabul\",\"languages\":\"fa-AF,ps,uz-AF,tk\",\"geonameId\":1149361,\"south\":29.3770645357176,\"isoAlpha3\":\"AFG\",\"north\":38.4907920755748,\"fipsCode\":\"AF\",\"population\":\"29121286\",\"east\":74.8894511481168,\"isoNumeric\":\"004\",\"areaInSqKm\":\"647500.0\",\"countryCode\":\"AF\",\"west\":60.4720833972263,\"countryName\":\"Afghanistan\",\"continentName\":\"Asien\",\"currencyCode\":\"AFN\"}]}";
        }
    }
}