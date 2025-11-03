using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using NUnit.Framework;

namespace HaemophilusWeb.Services
{
    public class IrisPubMlstServiceTests
    {
        [Test]
        public void GetIsolateByReference_AddsAuthenticationParameters()
        {
            var postUrl = string.Empty;
            var getUrls = new List<string>();
            var getResponses = new List<string>()
            {
                "{\"oauth_token\":\"TheSessionToken\",\"oauth_token_secret\":\"TheSessionTokenSecret\"}",
                DeserializeFromResource("HaemophilusWeb.TestData.PubMlstIsolateById.json"),
                "{\"records\":1,\"allele_ids\":[{\"\'16S_rDNA\":72}]}"
            };

            string GetCall(string url)
            {
                var response = getResponses[getUrls.Count];
                getUrls.Add(url);
                return response;
            }

            string PostCall(string url, Dictionary<string, string> _)
            {
                postUrl = url;
                return "{\"isolates\":[\"https://rest.pubmlst.org/db/pubmlst_neisseria_iris/isolates/93683\"],\"records\": 1}";
            }

            var authorization = new PubMlstAuthorization
            {
                AccessToken = "TheAccessToken",
                AccessTokenSecret = "TheAccessTokenSecret",
                ConsumerKey = "TheConsumerKey",
                ConsumerSecret = "TheConsumerSecret"
            };

            var sut = new IrisPubMlstService(authorization, GetCall, PostCall);

            sut.GetIsolateByReference("DE14505");

            getUrls[0].Should().Contain("oauth_token=TheAccessToken");
            getUrls[0].Should().Contain("TheConsumerKey");

            getUrls[1].Should().Contain("oauth_token=TheSessionToken");
            getUrls[1].Should().Contain("TheConsumerKey");
            
            getUrls[2].Should().Contain("oauth_token=TheSessionToken");
            getUrls[2].Should().Contain("TheConsumerKey");

            postUrl.Should().Contain("TheConsumerKey");
            postUrl.Should().Contain("oauth_token=TheSessionToken");
        }

        [Test]
        [Explicit]
        [Category("Integration")]
        public void GetIsolateByReference_ExistingIsolateWithoutMock_FieldsAreSet()
        {
            var authorization = new PubMlstAuthorization()
            {
                AccessToken = "<RealAccessToken>",
                AccessTokenSecret = "<RealAccessTokenSecret>",
                ConsumerKey = "<RealConsumerKey>",
                ConsumerSecret = "<RealConsumerSecret>"
            };
            var sut = new IrisPubMlstService(authorization);

            var isolate = sut.GetIsolateByReference("DE14500");

            isolate.PorAVr1.Should().Be("7-2");
            isolate.PorAVr2.Should().Be("4");
            isolate.SequenceType.Should().Be("41");
            isolate.ClonalComplex.Should().Be("ST-41/44 complex");
        }

        private static string DeserializeFromResource(string resourceName)
        {
            using (var stream = typeof(NeisseriaPubMlstServiceTests).Assembly.GetManifestResourceStream(resourceName))
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
    }
}