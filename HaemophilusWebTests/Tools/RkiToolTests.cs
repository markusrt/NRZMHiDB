using System;
using FluentAssertions;
using HaemophilusWeb.Models;
using NUnit.Framework;

namespace HaemophilusWeb.Tools
{
    public class RkiToolTests
    {
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
            var rkiTool = new RkiTool(s => "{\"query\":{\"count\":1,\"created\":\"2017-04-19T20:50:17Z\",\"lang\":\"de-DE\",\"results\":{\"postresult\":{\"div\":[{\"class\":\"tab-row\",\"span\":\"Adresse:\",\"br\":null,\"textarea\":{\"class\":\"tbxAdress\",\"id\":\"tbxAdress\",\"rows\":\"4\",\"content\":\"Landratsamt Forchheim\\r\\nGesundheitsamt\\r\\nAm Streckerplatz 3\\r\\n91301 Forchheim\"}},{\"class\":\"tab-row\",\"span\":\"Telefon:\",\"br\":null,\"textarea\":{\"class\":\"textbox\",\"cols\":\"20\",\"id\":\"tbxPhone\",\"name\":\"SelectedGA.Phone\",\"rows\":\"2\",\"content\":\"09191 86-3504\"}},{\"class\":\"tab-row\",\"span\":\"Fax:\",\"br\":null,\"textarea\":{\"class\":\"textbox\",\"cols\":\"20\",\"id\":\"tbxFax\",\"name\":\"SelectedGA.Fax\",\"rows\":\"2\",\"content\":\"09191 86-3508\"}},{\"class\":\"tab-row\",\"span\":\"E-Mail:  \",\"a\":{\"href\":\"mailto:Gesundheitsamt@lra-fo.de\",\"content\":\"Gesundheitsamt@lra-fo.de\"}}]}}}}");

            var healthOffice = rkiTool.QueryHealthOffice("91301");

            healthOffice.ShouldBeEquivalentTo(expectedHealthOffice);
        }

        [Test]
        public void QueryHealthOffice_InvalidPostalCode_ReturnsNull()
        {
            var rkiTool = new RkiTool(s => "{\"query\":{\"count\":1,\"created\":\"2017-04-19T21:13:23Z\",\"lang\":\"de-DE\",\"results\":{\"postresult\":null}}}");

            var healthOffice = rkiTool.QueryHealthOffice("111111");

            healthOffice.Should().BeNull();
        }

        [Test]
        public void QueryHealthOffice_ThrowsException_ReturnsNull()
        {
            var rkiTool = new RkiTool(s => throw new Exception());

            var healthOffice = rkiTool.QueryHealthOffice("111111");

            healthOffice.Should().BeNull();
        }
    }
}