using System;
using System.Collections.Generic;
using System.Net;
using FluentAssertions;
using HaemophilusWeb.Migrations;
using HaemophilusWeb.Utils;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace HaemophilusWeb.Services
{
    public class GeonamesServiceTest
    {
        [Test]
        public void QueryPostalCode_NoMatch_ReturnsEmptyArray()
        {
            var service = new GeonamesService(GetUrlReturns404);

            var queryResult = service.QueryByPostalCode("90");

            var jsonObject = JObject.Parse(queryResult);
            jsonObject["postalcodes"].As<JArray>().Should().HaveCount(0);
        }

        [Test]
        public void QueryPostalCode_UnexpectedResult_ReturnsEmptyArray()
        {
            var service = new GeonamesService(GetUrlUnexptetedResult);

            var queryResult = service.QueryByPostalCode("06773");

            var jsonObject = JObject.Parse(queryResult);
            jsonObject["postalcodes"].As<JArray>().Should().HaveCount(0);
        }


        [TestCase(null)]
        [TestCase("")]
        [TestCase("Forchheim")]
        public void QueryPostalCode_SingleMatch_ReturnsOneEntry(string placeName)
        {
            var service = new GeonamesService(GetUrlReturningSingleResult);

            var queryResult = service.QueryByPostalCode("91301", placeName);

            queryResult.Should().NotBeEmpty();
            var jsonObject = JObject.Parse(queryResult);
            jsonObject["postalcodes"].As<JArray>().Should().HaveCount(1);
        }

        [Test]
        public void QueryPostalCode_MultipleMatches_ReturnsAllEntries()
        {
            var service = new GeonamesService(GetUrlReturningMultipleResults);

            var queryResult = service.QueryByPostalCode("06773");

            queryResult.Should().NotBeEmpty();
            var jsonObject = JObject.Parse(queryResult);
            jsonObject["postalcodes"].As<JArray>().Should().HaveCount(1);
        }

        [Test]
        public void QueryPostalCodeAndPlaceName_MultipleMatches_ReturnsAllEntries()
        {
            var service = new GeonamesService(GetUrlReturningMultipleResults);

            var queryResult = service.QueryByPostalCode("06773", "Gossa");

            queryResult.Should().NotBeEmpty();
            var jsonObject = JObject.Parse(queryResult);
            jsonObject["postalcodes"].As<JArray>().Should().HaveCount(1);
        }

        private static string GetUrlReturns404(string arg)
        {
            throw new WebException();
        }

        private static string GetUrlReturningSingleResult(string arg)
        {
            return
                "{\"postalcodes\":[{\"adminCode2\":\"094\",\"adminCode3\":\"09474\",\"adminName3\":\"Landkreis Forchheim\",\"adminCode1\":\"BY\",\"adminName2\":\"Upper Franconia\",\"lng\":11.058769226074219,\"countryCode\":\"DE\",\"postalcode\":\"91301\",\"adminName1\":\"Bayern\",\"placeName\":\"Forchheim\",\"lat\":49.717542888321425}]}";
        }

        private static string GetUrlUnexptetedResult(string arg)
        {
            return
                "{\"foo\": \"bar\"}";
        }

        private static string GetUrlReturningMultipleResults(string arg)
        {
            return
                "{\"postalcodes\":[{\"adminCode2\":\"00\",\"adminCode3\":\"15082\",\"adminName3\":\"Anhalt-Bitterfeld\",\"adminCode1\":\"ST\",\"lng\":12.444217,\"countryCode\":\"DE\",\"postalcode\":\"06773\",\"adminName1\":\"Sachsen-Anhalt\",\"placeName\":\"Gossa\",\"lat\":51.669489},{\"adminCode2\":\"00\",\"adminCode3\":\"15091\",\"adminName3\":\"Landkreis Wittenberg\",\"adminCode1\":\"ST\",\"lng\":12.456513,\"countryCode\":\"DE\",\"postalcode\":\"06773\",\"adminName1\":\"Sachsen-Anhalt\",\"placeName\":\"Gräfenhainichen\",\"lat\":51.728924},{\"adminCode2\":\"00\",\"adminCode3\":\"15082\",\"adminName3\":\"Anhalt-Bitterfeld\",\"adminCode1\":\"ST\",\"lng\":12.451245,\"countryCode\":\"DE\",\"postalcode\":\"06773\",\"adminName1\":\"Sachsen-Anhalt\",\"placeName\":\"Gröbern\",\"lat\":51.688557},{\"adminCode2\":\"00\",\"adminCode3\":\"15091\",\"adminName3\":\"Landkreis Wittenberg\",\"adminCode1\":\"ST\",\"lng\":12.414775,\"countryCode\":\"DE\",\"postalcode\":\"06773\",\"adminName1\":\"Sachsen-Anhalt\",\"placeName\":\"Jüdenberg\",\"lat\":51.750499},{\"adminCode2\":\"00\",\"adminCode3\":\"15091\",\"adminName3\":\"Landkreis Wittenberg\",\"adminCode1\":\"ST\",\"lng\":12.597076023391,\"countryCode\":\"DE\",\"postalcode\":\"06773\",\"adminName1\":\"Sachsen-Anhalt\",\"placeName\":\"Kemberg\",\"lat\":51.800135501354},{\"adminCode2\":\"00\",\"adminCode3\":\"15091\",\"adminName3\":\"Landkreis Wittenberg\",\"adminCode1\":\"ST\",\"lng\":12.5833,\"countryCode\":\"DE\",\"postalcode\":\"06773\",\"adminName1\":\"Sachsen-Anhalt\",\"placeName\":\"Kemberg Bergwitz\",\"lat\":51.8},{\"adminCode2\":\"00\",\"adminCode3\":\"15091\",\"adminName3\":\"Landkreis Wittenberg\",\"adminCode1\":\"ST\",\"lng\":12.574561403508,\"countryCode\":\"DE\",\"postalcode\":\"06773\",\"adminName1\":\"Sachsen-Anhalt\",\"placeName\":\"Kemberg Klitzschena\",\"lat\":51.817073170731},{\"adminCode2\":\"00\",\"adminCode3\":\"15091\",\"adminName3\":\"Landkreis Wittenberg\",\"adminCode1\":\"ST\",\"lng\":12.514526,\"countryCode\":\"DE\",\"postalcode\":\"06773\",\"adminName1\":\"Sachsen-Anhalt\",\"placeName\":\"Radis\",\"lat\":51.75226},{\"adminCode2\":\"00\",\"adminCode3\":\"15091\",\"adminName3\":\"Landkreis Wittenberg\",\"adminCode1\":\"ST\",\"lng\":12.602363,\"countryCode\":\"DE\",\"postalcode\":\"06773\",\"adminName1\":\"Sachsen-Anhalt\",\"placeName\":\"Rotta\",\"lat\":51.764391},{\"adminCode2\":\"00\",\"adminCode3\":\"15091\",\"adminName3\":\"Landkreis Wittenberg\",\"adminCode1\":\"ST\",\"lng\":12.537213,\"countryCode\":\"DE\",\"postalcode\":\"06773\",\"adminName1\":\"Sachsen-Anhalt\",\"placeName\":\"Schköna\",\"lat\":51.68101},{\"adminCode2\":\"00\",\"adminCode3\":\"15091\",\"adminName3\":\"Landkreis Wittenberg\",\"adminCode1\":\"ST\",\"lng\":12.532124,\"countryCode\":\"DE\",\"postalcode\":\"06773\",\"adminName1\":\"Sachsen-Anhalt\",\"placeName\":\"Selbitz\",\"lat\":51.814621},{\"adminCode2\":\"00\",\"adminCode3\":\"15091\",\"adminName3\":\"Landkreis Wittenberg\",\"adminCode1\":\"ST\",\"lng\":12.556699,\"countryCode\":\"DE\",\"postalcode\":\"06773\",\"adminName1\":\"Sachsen-Anhalt\",\"placeName\":\"Uthausen\",\"lat\":51.764564}]}";
        }
    }
}