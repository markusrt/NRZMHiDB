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
        public void QueryPostalCodeAndPlaceName_MultipleMatches_ReturnsBestMatch()
        {
            var service = new GeonamesService(GetUrlReturningMultipleResults);

            var queryResult = service.QueryByPostalCode("06773", "Gossa");

            queryResult.Should().NotBeEmpty();
            var jsonObject = JObject.Parse(queryResult);
            var jArray = jsonObject["postalcodes"].As<JArray>();
            jArray.Should().HaveCount(1);
            jArray.First.Value<string>("placeName").Should().Be("Gossa");
        }

        // specs/022 FR-005, FR-006, US1.3, US1.5: without place name the first returned place is taken, with its
        // postal code, place name, county name (adminName3) and state short code (adminCode1)
        [Test]
        public void QueryPostalCode_MultipleMatchesWithoutPlaceName_ReturnsFirstEntryWithItsFields()
        {
            var service = new GeonamesService(GetUrlReturningMultipleResults);

            var queryResult = service.QueryByPostalCode("06773");

            var place = JObject.Parse(queryResult)["postalcodes"].As<JArray>().Should().ContainSingle().Subject;
            place.Value<string>("placeName").Should().Be("Gossa");
            place.Value<string>("postalcode").Should().Be("06773");
            place.Value<string>("adminName3").Should().Be("Anhalt-Bitterfeld");
            place.Value<string>("adminCode1").Should().Be("ST");
        }

        // specs/022 FR-005, US1.4: a place name that does not equal a returned name exactly (case, prefix,
        // trailing space, "Unbekannt") selects no place
        [TestCase("gossa")]
        [TestCase("Goss")]
        [TestCase("Gossa ")]
        [TestCase("Unbekannt")]
        public void QueryPostalCodeAndPlaceName_WithoutExactlyEqualName_ReturnsEmptyArray(string placeName)
        {
            var service = new GeonamesService(GetUrlReturningMultipleResults);

            var queryResult = service.QueryByPostalCode("06773", placeName);

            JObject.Parse(queryResult)["postalcodes"].As<JArray>().Should().BeEmpty();
        }

        // specs/022 FR-004, US1.1: the lookup asks for the places of the postal code in Germany
        [Test]
        public void QueryPostalCode_AsksLookupForPostalCodeInGermany()
        {
            string calledUrl = null;
            var service = new GeonamesService(url =>
            {
                calledUrl = url;
                return GetUrlReturningSingleResult(url);
            });

            service.QueryByPostalCode("91301", "Forchheim");

            calledUrl.Should().Contain("/postalCodeLookupJSON?country=DE&");
            calledUrl.Should().EndWith("&postalcode=91301");
            calledUrl.Should().NotContain("Forchheim");
        }

        // specs/022 FR-002, US2.1, US2.2: suggestions are searched by prefix in Germany and the service's answer is
        // passed on unchanged (no own limit, service order)
        [Test]
        public void QueryPostalCodePrefix_AsksSearchInGermanyAndPassesAnswerThrough()
        {
            const string answer =
                "{\"postalCodes\":[{\"postalCode\":\"91320\",\"placeName\":\"Ebermannstadt\"},{\"postalCode\":\"91301\",\"placeName\":\"Forchheim\"},{\"postalCode\":\"91322\",\"placeName\":\"Gräfenberg\"}]}";
            string calledUrl = null;
            var service = new GeonamesService(url =>
            {
                calledUrl = url;
                return answer;
            });

            var queryResult = service.QueryByPostalCodePrefix("913");

            calledUrl.Should().Contain("/postalCodeSearchJSON?country=DE&");
            calledUrl.Should().EndWith("&postalcode_startsWith=913");
            JToken.DeepEquals(JObject.Parse(queryResult), JObject.Parse(answer)).Should().BeTrue();
        }

        // specs/022 FR-009, Edge Cases "Missing setting": without a GeonamesAccount setting (the test configuration
        // has none) the service is addressed with an empty account name
        [Test]
        public void QueryPostalCode_WithoutAccountSetting_SendsEmptyAccountName()
        {
            Assume.That(System.Configuration.ConfigurationManager.AppSettings["GeonamesAccount"], Is.Null);
            var calledUrls = new List<string>();
            var service = new GeonamesService(url =>
            {
                calledUrls.Add(url);
                return GetUrlReturningSingleResult(url);
            });

            service.QueryByPostalCode("91301");
            service.QueryByPostalCodePrefix("913");

            calledUrls.Should().HaveCount(2).And.OnlyContain(u => u.Contains("&username=&"));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("Forchheim")]
        public void QueryCoordinate_SingleMatch_ReturnsOneEntry(string placeName)
        {
            var service = new GeonamesService(GetUrlReturningSingleResult);

            var queryResult = service.QueryCoordinateByPostalCode("91301", placeName);

            queryResult.Should().NotBeNull();
            queryResult?.Latitude.Should().Be(49.717542888321425);
            queryResult?.Longitude.Should().Be(11.058769226074219);
        }

        [Test]
        public void QueryCoordinate_MultipleMatches_ReturnsBestMatch()
        {
            var service = new GeonamesService(GetUrlReturningMultipleResults);

            var queryResult = service.QueryCoordinateByPostalCode("06773", "Gossa");

            queryResult.Should().NotBeNull();
            queryResult?.Latitude.Should().Be(51.669489);
            queryResult?.Longitude.Should().Be(12.444217);
        }


        [Test]
        public void QueryCoordinate_NoMatch_ReturnsNull()
        {
            var service = new GeonamesService(GetUrlReturns404);

            var queryResult = service.QueryCoordinateByPostalCode("90");

            queryResult.Should().BeNull();
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