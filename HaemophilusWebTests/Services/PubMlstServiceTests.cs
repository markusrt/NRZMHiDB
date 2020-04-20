using System.Collections.Generic;
using System.Net;
using FluentAssertions;
using NUnit.Framework;

namespace HaemophilusWeb.Services
{
    public class PubMlstServiceTests
    {
        [Test]
        public void NeisseriaIsolates_InexisingIsolate_ReturnsEmptyResult()
        {
            var service = new PubMlstService(GetUrlReturns404, PostUrlReturns404);

            var isolate = service.GetIsolateById(0);

            isolate.Should().BeNull();
        }

        [Test]
        public void GetIsolateById_ExistingIsolate_AllFieldsAreSet()
        {
            var service = new PubMlstService(GetUrlReturningIsolate, PostUrlReturns404);

            var isolate = service.GetIsolateById(1234);

            isolate.PubMlstId.Should().Be(1234);
            isolate.PorAVr1.Should().Be("5");
            isolate.PorAVr2.Should().Be("2");
            isolate.FetAVr.Should().Be("F3-6");
            isolate.PorB.Should().Be("2-2");
            isolate.Fhbp.Should().Be("1511");
            isolate.Nhba.Should().Be("20");
            isolate.NadA.Should().Be("");
            isolate.PenA.Should().Be("1");
            isolate.GyrA.Should().Be("4");
            isolate.ParC.Should().Be("1");
            isolate.ParE.Should().Be("1");
            isolate.RpoB.Should().Be("4");
            isolate.RplF.Should().Be("1");
            isolate.SequenceType.Should().Be("22");
            isolate.ClonalComplex.Should().Be("ST-22 complex");
        }

        [Test]
        public void GetIsolateByReference_InexistingIsolate_ReturnsNull()
        {
            var controller = new PubMlstService(GetUrlReturns404, PostUrlReturns404);

            var isolate = controller.GetIsolateByReference("DE14");

            isolate.Should().BeNull();
        }

        [Test]
        public void GetIsolateByReference_ExistingIsolate_FieldsAreSet()
        {
            var controller = new PubMlstService(GetUrlReturningIsolate, PostUrlReturnsResult);

            var isolate = controller.GetIsolateByReference("DE14505");

            isolate.PubMlstId.Should().Be(93683);
            isolate.PorAVr1.Should().Be("5");
            isolate.PorAVr2.Should().Be("2");
            isolate.SequenceType.Should().Be("22");
            isolate.ClonalComplex.Should().Be("ST-22 complex");
        }

        [Test]
        public void GetIsolateByReference_FieldsPropertyNull_OtherValuesAreSet()
        {
            var controller = new PubMlstService(GetUrlReturningIsolateWithoutFields, PostUrlReturnsResult);

            var isolate = controller.GetIsolateByReference("DE14505");

            isolate.PubMlstId.Should().Be(93683);
            isolate.PorAVr1.Should().Be("5");
            isolate.PorAVr2.Should().Be("2");
            isolate.SequenceType.Should().BeNull();
            isolate.ClonalComplex.Should().BeNull();
        }

        [Test]
        public void GetIsolateByReference_FieldsMissing_OtherValuesAreSet()
        {
            var controller = new PubMlstService(GetUrlReturningIsolateWithMissingFields, PostUrlReturnsResult);

            var isolate = controller.GetIsolateByReference("DE14505");

            isolate.PubMlstId.Should().Be(93683);
            isolate.PorAVr1.Should().Be("5");
            isolate.PorAVr2.Should().Be("2");
            isolate.SequenceType.Should().BeNull();
            isolate.ClonalComplex.Should().BeNull();
        }

        [Test]
        [Explicit]
        [Category("Integration")]
        public void GetIsolateById_ExistingIsolateWithoutMock_FieldsAreSet()
        {
            var controller = new PubMlstService();

            var isolate = controller.GetIsolateById(93683);

            isolate.PorAVr1.Should().Be("5-1");
            isolate.PorAVr2.Should().Be("10-1");
            isolate.SequenceType.Should().Be("11");
            isolate.ClonalComplex.Should().Be("ST-11 complex");
        }

        [Test]
        [Explicit]
        [Category("Integration")]
        public void GetIsolateByReference_InexistingIsolateWithoutMock_ReturnsNull()
        {
            var controller = new PubMlstService();

            var isolate = controller.GetIsolateByReference("DE14");

            isolate.Should().BeNull();
        }

        [Test]
        [Explicit]
        [Category("Integration")]
        public void GetIsolateByReference_ExistingIsolateWithoutMock_FieldsAreSet()
        {
            var controller = new PubMlstService();

            var isolate = controller.GetIsolateByReference("DE14505");

            isolate.PorAVr1.Should().Be("5-1");
            isolate.PorAVr2.Should().Be("10-1");
            isolate.SequenceType.Should().Be("11");
            isolate.ClonalComplex.Should().Be("ST-11 complex");
        }


        private static string PostUrlReturns404(string url, Dictionary<string, string> parameters)
        {
            return "{\"isolates\": [],\"records\": 0}";
        }


        private static string PostUrlReturnsResult(string arg1, Dictionary<string, string> arg2)
        {
            return
                "{\"isolates\":[\"http://rest.pubmlst.org/db/pubmlst_neisseria_isolates/isolates/93683\"],\"records\": 1}";
        }

        private static string GetUrlReturns404(string arg)
        {
            throw new WebException();
        }

        private static string GetUrlReturningIsolate(string arg)
        {
            return arg.Contains("/allele_ids?return_all=1") 
            ? "{\"records\":63,\"allele_ids\":[{\"\'16S_rDNA\":72},{\"abcZ\":2},{\"adk\":3},{\"aroE\":4},{\"aspA\":11},{\"carB\":6},{\"dhpS\":11},{\"FetA_VR\":\"F3-6\"},{\"FfrR_Regulon\":7},{\"\'fHbp\":1511},{\"fHbp_peptide\":1156},{\"fumC\":3},{\"gdh\":8},{\"glnA\":2},{\"gyrA\":4},{\"hmbR\":9},{\"mtgA\":4},{\"NG_ponA\":100},{\"NEIS1525\":1},{\"NEIS1600\":1},{\"NG_porB\":34},{\"NHBA_peptide\":20},{\"nrrF\":26},{\"pdhC\":4},{\"penA\":1},{\"pgm\":6},{\"pilA\":4},{\"pip\":3},{\"\'porA\":9},{\"PorA_VR1\":\"5\"},{\"PorA_VR2\":\"2\"},{\"PorA_VR3\":\"36-2\"},{\"\'porB\":\"2-2\"},{\"pro_NEIS0350\":1},{\"pro_NEIS0488\":3},{\"pro_NEIS0763\":7},{\"pro_NEIS1635\":5},{\"rpiA\":15},{\"\'rplF\":1},{\"rpoB\":4},{\"sRNA25a\":1},{\"sRNA25b\":3},{\"talA\":6},{\"tRNA-ala\":[1,2]},{\"tRNA-arg\":[1,2,3]},{\"tRNA-asn\":1},{\"tRNA-asp\":1},{\"tRNA-cys\":1},{\"tRNA-fmet\":1},{\"tRNA-gln\":[1,2]},{\"tRNA-glu\":1},{\"tRNA-gly\":[1,2]},{\"tRNA-his\":[1,2]},{\"tRNA-ile\":[1,2]},{\"tRNA-leu\":[1,3,4,5,6,7]},{\"tRNA-lys\":[1,2]},{\"tRNA-met\":1},{\"tRNA-phe\":1},{\"tRNA-pro\":[1,2,3]},{\"tRNA-ser\":[1,2,3,4]},{\"tRNA-thr\":[2,3]},{\"tRNA-trp\":[2,3]},{\"tRNA-tyr\":[1,16]},{\"tRNA-val\":[1,2]}]}"
            : "{\"schemes\":[{\"allele_ids\":\"http://rest.pubmlst.org/db/pubmlst_neisseria_isolates/isolates/93683/schemes/1/allele_ids\",\"loci_designated_count\":7,\"description\":\"MLST\",\"fields\":{\"ST\":22,\"clonal_complex\":\"ST-22 complex\"},\"full_designations\":\"http://rest.pubmlst.org/db/pubmlst_neisseria_isolates/isolates/93683/schemes/1/allele_designations\"}]}";
        }

        private static string GetUrlReturningIsolateWithMissingFields(string arg)
        {
            return arg.Contains("/allele_ids?return_all=1")
                ? "{\"records\":63,\"allele_ids\":[{\"\'16S_rDNA\":72},{\"abcZ\":2},{\"adk\":3},{\"aroE\":4},{\"aspA\":11},{\"carB\":6},{\"dhpS\":11},{\"FetA_VR\":\"F3-6\"},{\"FfrR_Regulon\":7},{\"\'fHbp\":1511},{\"fHbp_peptide\":1156},{\"fumC\":3},{\"gdh\":8},{\"glnA\":2},{\"gyrA\":4},{\"hmbR\":9},{\"mtgA\":4},{\"NG_ponA\":100},{\"NEIS1525\":1},{\"NEIS1600\":1},{\"NG_porB\":34},{\"NHBA_peptide\":20},{\"nrrF\":26},{\"pdhC\":4},{\"penA\":1},{\"pgm\":6},{\"pilA\":4},{\"pip\":3},{\"\'porA\":9},{\"PorA_VR1\":\"5\"},{\"PorA_VR2\":\"2\"},{\"PorA_VR3\":\"36-2\"},{\"\'porB\":\"2-2\"},{\"pro_NEIS0350\":1},{\"pro_NEIS0488\":3},{\"pro_NEIS0763\":7},{\"pro_NEIS1635\":5},{\"rpiA\":15},{\"\'rplF\":1},{\"rpoB\":4},{\"sRNA25a\":1},{\"sRNA25b\":3},{\"talA\":6},{\"tRNA-ala\":[1,2]},{\"tRNA-arg\":[1,2,3]},{\"tRNA-asn\":1},{\"tRNA-asp\":1},{\"tRNA-cys\":1},{\"tRNA-fmet\":1},{\"tRNA-gln\":[1,2]},{\"tRNA-glu\":1},{\"tRNA-gly\":[1,2]},{\"tRNA-his\":[1,2]},{\"tRNA-ile\":[1,2]},{\"tRNA-leu\":[1,3,4,5,6,7]},{\"tRNA-lys\":[1,2]},{\"tRNA-met\":1},{\"tRNA-phe\":1},{\"tRNA-pro\":[1,2,3]},{\"tRNA-ser\":[1,2,3,4]},{\"tRNA-thr\":[2,3]},{\"tRNA-trp\":[2,3]},{\"tRNA-tyr\":[1,16]},{\"tRNA-val\":[1,2]}]}"
                : "{\"schemes\":[{\"allele_ids\":\"http://rest.pubmlst.org/db/pubmlst_neisseria_isolates/isolates/93683/schemes/1/allele_ids\",\"loci_designated_count\":7,\"description\":\"MLST\",\"fields\":{},\"full_designations\":\"http://rest.pubmlst.org/db/pubmlst_neisseria_isolates/isolates/93683/schemes/1/allele_designations\"}]}";
        }

        private static string GetUrlReturningIsolateWithoutFields(string arg)
        {
            return arg.Contains("/allele_ids?return_all=1")
                ? "{\"records\":63,\"allele_ids\":[{\"\'16S_rDNA\":72},{\"abcZ\":2},{\"adk\":3},{\"aroE\":4},{\"aspA\":11},{\"carB\":6},{\"dhpS\":11},{\"FetA_VR\":\"F3-6\"},{\"FfrR_Regulon\":7},{\"\'fHbp\":1511},{\"fHbp_peptide\":1156},{\"fumC\":3},{\"gdh\":8},{\"glnA\":2},{\"gyrA\":4},{\"hmbR\":9},{\"mtgA\":4},{\"NG_ponA\":100},{\"NEIS1525\":1},{\"NEIS1600\":1},{\"NG_porB\":34},{\"NHBA_peptide\":20},{\"nrrF\":26},{\"pdhC\":4},{\"penA\":1},{\"pgm\":6},{\"pilA\":4},{\"pip\":3},{\"\'porA\":9},{\"PorA_VR1\":\"5\"},{\"PorA_VR2\":\"2\"},{\"PorA_VR3\":\"36-2\"},{\"\'porB\":\"2-2\"},{\"pro_NEIS0350\":1},{\"pro_NEIS0488\":3},{\"pro_NEIS0763\":7},{\"pro_NEIS1635\":5},{\"rpiA\":15},{\"\'rplF\":1},{\"rpoB\":4},{\"sRNA25a\":1},{\"sRNA25b\":3},{\"talA\":6},{\"tRNA-ala\":[1,2]},{\"tRNA-arg\":[1,2,3]},{\"tRNA-asn\":1},{\"tRNA-asp\":1},{\"tRNA-cys\":1},{\"tRNA-fmet\":1},{\"tRNA-gln\":[1,2]},{\"tRNA-glu\":1},{\"tRNA-gly\":[1,2]},{\"tRNA-his\":[1,2]},{\"tRNA-ile\":[1,2]},{\"tRNA-leu\":[1,3,4,5,6,7]},{\"tRNA-lys\":[1,2]},{\"tRNA-met\":1},{\"tRNA-phe\":1},{\"tRNA-pro\":[1,2,3]},{\"tRNA-ser\":[1,2,3,4]},{\"tRNA-thr\":[2,3]},{\"tRNA-trp\":[2,3]},{\"tRNA-tyr\":[1,16]},{\"tRNA-val\":[1,2]}]}"
                : "{\"schemes\":[{\"allele_ids\":\"http://rest.pubmlst.org/db/pubmlst_neisseria_isolates/isolates/93683/schemes/1/allele_ids\",\"loci_designated_count\":7,\"description\":\"MLST\",\"full_designations\":\"http://rest.pubmlst.org/db/pubmlst_neisseria_isolates/isolates/93683/schemes/1/allele_designations\"}]}";
        }
    }
}