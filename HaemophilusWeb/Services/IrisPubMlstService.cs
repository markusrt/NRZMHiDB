using System;
using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json;
using OAuth;
using static HaemophilusWeb.Utils.HttpClientWrapper;

namespace HaemophilusWeb.Services
{
    public class IrisPubMlstService : PubMlstService
    {
        private readonly PubMlstAuthorization _authorization;

        private PubMlstSessionToken _sessionToken;

        protected override string Database => "pubmlst_neisseria_iris";
        
        private string SessionTokenUrl =>  $"{DbUrl}/oauth/get_session_token";

        public IrisPubMlstService(PubMlstAuthorization authorization) : this(authorization, CallUrlViaGet, CallUrlViaPost)
        {
        }

        public IrisPubMlstService(PubMlstAuthorization authorization, Func<string, string> callGetUrl, Func<string, Dictionary<string,string>, string> callPostUrl)
            : base(callGetUrl, callPostUrl)
        {
            _authorization = authorization;
        }

        protected override string GetSearchUrl(string isolateReference)
        {
            return CreateAuthUrl($"{IsolatesUrl}/search", HttpMethod.Post, new Dictionary<string, string>());
        }

        protected override string GetIdUrl(int id)
        {
            return CreateAuthUrl($"{IsolatesUrl}/{id}", HttpMethod.Get, new Dictionary<string, string>());
        }

        protected override string GetAlleleIdUrl(int id)
        {
            var alleleIdUrl = CreateAuthUrl($"{IsolatesUrl}/{id}/allele_ids", HttpMethod.Get,
                new Dictionary<string, string> { { "return_all", "1" } });
            return $"{alleleIdUrl}return_all=1";
        }

        private string CreateAuthUrl(string requestUrl, HttpMethod method, IDictionary<string, string> parameters)
        {
            EnsureSessionIsEstablished();
            return CreateAuthUrl(requestUrl, method, _sessionToken.Token, _sessionToken.TokenSecret, parameters);
        }

        private string CreateAuthUrl(string requestUrl, HttpMethod method, string token, string tokenSecret, IDictionary<string, string> parameters) 
        {
            var oauth = OAuthRequest.ForProtectedResource(
                method.Method, _authorization.ConsumerKey, _authorization.ConsumerSecret, token, tokenSecret);
            oauth.RequestUrl = requestUrl;
            oauth.Method = method.Method;

            return $"{oauth.RequestUrl}?{oauth.GetAuthorizationQuery(parameters)}";
        }

        private void EnsureSessionIsEstablished()
        {
            if (_sessionToken != null)
            {
                return;
            }

            var sessionTokenRequestUrl = CreateAuthUrl(SessionTokenUrl, HttpMethod.Get, _authorization.AccessToken,
                _authorization.AccessTokenSecret, new Dictionary<string, string>());
            
            var tokenResponse = CallGetUrl(sessionTokenRequestUrl);
            _sessionToken = JsonConvert.DeserializeObject<PubMlstSessionToken>(tokenResponse);
        }
    }
}