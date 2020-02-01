using System.Net;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using FluentAssertions;
using HaemophilusWeb.Models;
using HaemophilusWeb.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NSubstitute;
using NUnit.Framework;

namespace HaemophilusWeb.Controllers
{
    [TestFixture]
    public class PubMlstControllerTests
    {
        [Test]
        public void NeisseriaIsolates_InexisingIsolate_ReturnsNotFound()
        {
            var service = Substitute.For<PubMlstService>();
            var controller = new PubMlstController(service);

            var isolate = controller.NeisseriaIsolates("ABC");

            isolate.Should().BeOfType<HttpNotFoundResult>();
        }

        [Test]
        public void NeisseriaIsolates_ExisingIsolate_FieldsAreSet()
        {
            const string isolateReference = "DE1234";
            var service = Substitute.For<PubMlstService>();
            service.GetIsolateByReference(isolateReference).Returns(new NeisseriaPubMlstIsolate { SequenceType = "22" });

            var controller = new PubMlstController(service);

            var json = new JavaScriptSerializer().Serialize((controller.NeisseriaIsolates(isolateReference) as JsonResult)?.Data);
            var isolate = JsonConvert.DeserializeObject<NeisseriaPubMlstIsolate>(json);
            isolate.SequenceType.Should().Be("22");
           
        }


        private static string UrlReturns404(string arg)
        {
            throw new WebException();
        }

        private static string UrlReturnsIsolate35105(string arg)
        {
            return
                "{\"records\":63,\"allele_ids\":[{\"\'16S_rDNA\":72},{\"abcZ\":2},{\"adk\":3},{\"aroE\":4},{\"aspA\":11},{\"carB\":6},{\"dhpS\":11},{\"FetA_VR\":\"F3-6\"},{\"FfrR_Regulon\":7},{\"\'fHbp\":1511},{\"fHbp_peptide\":1156},{\"fumC\":3},{\"gdh\":8},{\"glnA\":2},{\"gyrA\":4},{\"hmbR\":9},{\"mtgA\":4},{\"NG_ponA\":100},{\"NEIS1525\":1},{\"NEIS1600\":1},{\"NG_porB\":34},{\"NHBA_peptide\":20},{\"nrrF\":26},{\"pdhC\":4},{\"penA\":1},{\"pgm\":6},{\"pilA\":4},{\"pip\":3},{\"\'porA\":9},{\"PorA_VR1\":\"5\"},{\"PorA_VR2\":\"2\"},{\"PorA_VR3\":\"36-2\"},{\"\'porB\":\"2-2\"},{\"pro_NEIS0350\":1},{\"pro_NEIS0488\":3},{\"pro_NEIS0763\":7},{\"pro_NEIS1635\":5},{\"rpiA\":15},{\"\'rplF\":1},{\"rpoB\":4},{\"sRNA25a\":1},{\"sRNA25b\":3},{\"talA\":6},{\"tRNA-ala\":[1,2]},{\"tRNA-arg\":[1,2,3]},{\"tRNA-asn\":1},{\"tRNA-asp\":1},{\"tRNA-cys\":1},{\"tRNA-fmet\":1},{\"tRNA-gln\":[1,2]},{\"tRNA-glu\":1},{\"tRNA-gly\":[1,2]},{\"tRNA-his\":[1,2]},{\"tRNA-ile\":[1,2]},{\"tRNA-leu\":[1,3,4,5,6,7]},{\"tRNA-lys\":[1,2]},{\"tRNA-met\":1},{\"tRNA-phe\":1},{\"tRNA-pro\":[1,2,3]},{\"tRNA-ser\":[1,2,3,4]},{\"tRNA-thr\":[2,3]},{\"tRNA-trp\":[2,3]},{\"tRNA-tyr\":[1,16]},{\"tRNA-val\":[1,2]}]}";
        }
    }
}