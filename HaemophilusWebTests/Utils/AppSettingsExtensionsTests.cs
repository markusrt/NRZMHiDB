using System.Collections.Specialized;
using FluentAssertions;
using NUnit.Framework;

namespace HaemophilusWeb.Utils
{
    public class AppSettingsExtensionsTests
    {
        // specs/019 FR-014, Remote databases: the IRIS credentials come from four named application settings
        [Test]
        public void GetIrisAuthentication_ReadsFourNamedSettings()
        {
            var settings = new NameValueCollection
            {
                { "IrisAccessToken", "synthetic-access-token" },
                { "IrisAccessTokenSecret", "synthetic-access-token-secret" },
                { "IrisConsumerKey", "synthetic-consumer-key" },
                { "IrisConsumerSecret", "synthetic-consumer-secret" }
            };

            var authorization = settings.GetIrisAuthentication();

            authorization.AccessToken.Should().Be("synthetic-access-token");
            authorization.AccessTokenSecret.Should().Be("synthetic-access-token-secret");
            authorization.ConsumerKey.Should().Be("synthetic-consumer-key");
            authorization.ConsumerSecret.Should().Be("synthetic-consumer-secret");
        }
    }
}
