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
            var rkiTool = new RkiTool(s => "{\"query\":{\"count\":7,\"created\":\"2017-06-05T10:03:47Z\",\"lang\":\"en-US\",\"results\":{\"input\":[{\"class\":\"form-control\",\"readonly\":\"readonly\",\"type\":\"text\",\"value\":\"09191 86-3504\"},{\"class\":\"form-control\",\"readonly\":\"readonly\",\"type\":\"text\",\"value\":\"09191 86-3508\"},{\"class\":\"form-control\",\"readonly\":\"readonly\",\"type\":\"text\",\"value\":\"Gesundheitsamt@lra-fo.de\"}],\"content\":\"Landratsamt Forchheim\\r\\nGesundheitsamt\\r\\n            Am Streckerplatz 3\\r\\n            91301 Forchheim\"}}}");

            var healthOffice = rkiTool.QueryHealthOffice("91301");

            healthOffice.ShouldBeEquivalentTo(expectedHealthOffice);
        }

        [Test]
        public void QueryHealthOffice_InvalidPostalCode_ReturnsNull()
        {
            var rkiTool = new RkiTool(s => "{\"query\":{\"count\":0,\"created\":\"2017-06-05T10:04:33Z\",\"lang\":\"en-US\",\"results\":null}}");

            var healthOffice = rkiTool.QueryHealthOffice("111111");

            healthOffice.Should().BeNull();
        }

        [Test]
        public void QueryHealthOffice_ThrowsException_ReturnsNull()
        {
            var rkiTool = new RkiTool(s => { throw new Exception(); });

            var healthOffice = rkiTool.QueryHealthOffice("111111");

            healthOffice.Should().BeNull();
        }
    }
}