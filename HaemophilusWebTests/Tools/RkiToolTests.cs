using System;
using System.IO;
using System.Reflection;
using FluentAssertions;
using HaemophilusWeb.Models;
using NUnit.Framework;

namespace HaemophilusWeb.Tools
{
    public class RkiToolTests
    {

        private string RkiToolQuerySingleResult;
        private string RkiToolQueryNoResult;
        private string RkiToolQueryMultipleResults;
        private string RkiToolQueryResultWithUmlaut;
        private string RkiToolQueryResultWithInvalidPhoneAndFax;

        [SetUp]
        public void LoadRkiToolQueryResult()
        {
            var assembly = Assembly.GetExecutingAssembly();

            RkiToolQuerySingleResult = GetResource(assembly, "HaemophilusWeb.Resources.RkiToolQuerySingleResult.html");
            RkiToolQueryNoResult = GetResource(assembly, "HaemophilusWeb.Resources.RkiToolQueryNoResult.html");
            RkiToolQueryMultipleResults = GetResource(assembly, "HaemophilusWeb.Resources.RkiToolQueryMultipleResults.html");
            RkiToolQueryResultWithUmlaut = GetResource(assembly, "HaemophilusWeb.Resources.RkiToolQueryResultWithUmlaut.html");
            RkiToolQueryResultWithInvalidPhoneAndFax = GetResource(assembly, "HaemophilusWeb.Resources.RkiToolQueryResultWithInvalidFaxAndPhone.html");
        }

        private static string GetResource(Assembly assembly, string resourceName)
        {
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        [Test]
        public void QueryHealthOffice_ValidPostalCode_ReturnsValidData()
        {
            var expectedHealthOffice = new HealthOffice
            {
                Address = "Landratsamt Forchheim\nGesundheitsamt\nAm Streckerplatz 3\n91301 Forchheim",
                Phone = "09191 86-3504",
                Fax = "09191 86-3508",
                Email = "Gesundheitsamt@lra-fo.de",
                PostalCode = "91301"
            };
            var rkiTool = new RkiTool(s => RkiToolQuerySingleResult);

            var healthOffice = rkiTool.QueryHealthOffice("91301");

            healthOffice.Should().BeEquivalentTo(expectedHealthOffice);
        }

        [Test]
        public void QueryHealthOffice_InvalidPostalCode_ReturnsNull()
        {
            var rkiTool = new RkiTool(s => RkiToolQueryNoResult);

            var healthOffice = rkiTool.QueryHealthOffice("111111");

            healthOffice.Should().BeNull();
        }

        [Test]
        public void QueryHealthOffice_MultipleResults_ReturnsFirst()
        {
            var expectedHealthOffice = new HealthOffice
            {
                Address = "Landratsamt Forchheim\nGesundheitsamt\nAm Streckerplatz 3\n91301 Forchheim",
                Phone = "09191 86-3504",
                Fax = "09191 86-3508",
                Email = "Gesundheitsamt@lra-fo.de",
                PostalCode = "91301"
            };
            var rkiTool = new RkiTool(s => RkiToolQueryMultipleResults);

            var healthOffice = rkiTool.QueryHealthOffice("91301");

            healthOffice.Should().BeEquivalentTo(expectedHealthOffice);
        }

        [Test]
        public void QueryHealthOffice_ThrowsException_ReturnsNull()
        {
            var rkiTool = new RkiTool(s => { throw new Exception(); });

            var healthOffice = rkiTool.QueryHealthOffice("111111");

            healthOffice.Should().BeNull();
        }

        [Test]
        public void QueryHealthOffice_AddressWithUmlaut_UnescapesHtml()
        {
            var rkiTool = new RkiTool(s => RkiToolQueryResultWithUmlaut);

            var healthOffice = rkiTool.QueryHealthOffice("96317");

            healthOffice.Should().NotBeNull();
            healthOffice.Address.Should()
                .Be("Landratsamt Kronach\nSachgebiet 36 - Gesundheitsamt\nGüterstrasse 18\n96317 Kronach");
        }

        [Test]
        public void QueryHealthOffice_InvalidPhoneAndFax_RemovesExcessSpaces()
        {
            var expectedHealthOffice = new HealthOffice
            {
                Address = "Landratsamt Forchheim\nGesundheitsamt\nAm Streckerplatz 3\n91301 Forchheim",
                Phone = "09191 86-3504",
                Fax = "09191 86-3508",
                Email = "Gesundheitsamt@lra-fo.de",
                PostalCode = "91301"
            };
            var rkiTool = new RkiTool(s => RkiToolQueryResultWithInvalidPhoneAndFax);

            var healthOffice = rkiTool.QueryHealthOffice("91301");

            healthOffice.Should().BeEquivalentTo(expectedHealthOffice);
        }
    }
}