using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using Breezy.Sdk.Payloads;
using Newtonsoft.Json;
using OAuth;

namespace Breezy.Sdk
{
    public partial class BreezyApiClient
    {
        private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        /// Gets an OAuth request token
        /// </summary>
        private OAuthToken GetRequestToken()
        {
            var request = OAuthRequest.ForRequestToken(_clientKey, _clientSecret);

            request.CallbackUrl = "oob";
            request.Method = "POST";
            request.RequestUrl = _apiUri.AbsoluteUri + ApiEndpoints.RequestToken;

            using (var http = new HttpClient())
            {
                http.DefaultRequestHeaders.Add("Authorization", request.GetAuthorizationHeader());

                var response = http.PostAsync(request.RequestUrl, null).Result;
                var responseBody = response.Content.ReadAsStringAsync().Result;

                if (response.StatusCode != HttpStatusCode.OK)
                    throw new BreezyApiException(
                        String.Format("Could not get request token.\r\nResponse status code: {0}.\r\nResponse: {1}",
                                      response.StatusCode, responseBody));

                var content = Util.ParseFormContent(responseBody);
                return new OAuthToken
                {
                    Token = content["oauth_token"],
                    TokenSecret = content["oauth_token_secret"]
                };
            }
        }

        /// <summary>
        /// Exchanges MDM auth key with a user email for an OAuth verifier
        /// </summary>
        private string GetOAuthVerifierWithMdmAuthKey(string mdmAuthKey, string userEmail, OAuthToken requestToken)
        {
            var message = new SsoAuthorizationMessage
            {
                MdmAuthKey = mdmAuthKey,
                ClientKey = _clientKey,
                Email = userEmail,
                Timestamp = (long)(DateTime.UtcNow - Epoch).TotalSeconds
            };
            message.Signature = Util.GetHmacSha256Signature(message.Email + message.Timestamp, _clientSecret);

            using (var http = new HttpClient())
            {
                var url = _apiUri.AbsoluteUri + ApiEndpoints.SsoAuthorize +
                          "?oauth_token=" + Uri.EscapeDataString(requestToken.Token);
                var requestContent = new StringContent(JsonConvert.SerializeObject(message), Encoding.UTF8, "application/json");

                var response = http.PostAsync(url, requestContent).Result;
                var responseBody = response.Content.ReadAsStringAsync().Result;

                if (response.StatusCode != HttpStatusCode.OK)
                    throw new BreezyApiException(
                        String.Format("Could not authorize with MDM auth key.\r\nResponse status code: {0}.\r\nResponse: {1}",
                                      response.StatusCode, responseBody));

                var content = Util.ParseFormContent(responseBody);
                return content["oauth_verifier"];
            }
        }

        /// <summary>
        /// Exchanges user email and password to verify request token
        /// </summary>
        private string GetOAuthVerifierWithPassword(string password, string userEmail, OAuthToken requestToken)
        {
            var message = new AuthorizationMessage
            {
                Password = password,
                Email = userEmail
            };

            using (var http = new HttpClient())
            {
                var url = _apiUri.AbsoluteUri + ApiEndpoints.Authorize +
                          "?oauth_token=" + Uri.EscapeDataString(requestToken.Token);
                var requestContent = new StringContent(JsonConvert.SerializeObject(message), Encoding.UTF8, "application/json");

                var response = http.PostAsync(url, requestContent).Result;
                var responseBody = response.Content.ReadAsStringAsync().Result;

                if (response.StatusCode != HttpStatusCode.OK)
                    throw new BreezyApiException(
                        String.Format("Could not authorize with user email and password.\r\nResponse status code: {0}.\r\nResponse: {1}",
                                      response.StatusCode, responseBody));

                var content = Util.ParseFormContent(responseBody);
                return content["oauth_verifier"];
            }
        }

        /// <summary>
        /// Gets an OAuth access token
        /// </summary>
        private OAuthToken GetAccessToken(OAuthToken requestToken, string oauthVerifier)
        {
            var request = OAuthRequest.ForAccessToken(_clientKey, _clientSecret,
                                                      requestToken.Token, requestToken.TokenSecret,
                                                      oauthVerifier);

            request.Method = "POST";
            request.RequestUrl = _apiUri.AbsoluteUri + ApiEndpoints.AccessToken;

            using (var http = new HttpClient())
            {
                http.DefaultRequestHeaders.Add("Authorization", request.GetAuthorizationHeader());

                var response = http.PostAsync(request.RequestUrl, null).Result;
                var responseBody = response.Content.ReadAsStringAsync().Result;

                if (response.StatusCode != HttpStatusCode.OK)
                    throw new BreezyApiException(
                        String.Format("Could not get access token.\r\nResponse status code: {0}.\r\nResponse: {1}",
                                      response.StatusCode, responseBody));

                var content = Util.ParseFormContent(responseBody);
                return new OAuthToken
                {
                    Token = content["oauth_token"],
                    TokenSecret = content["oauth_token_secret"]
                };
            }
        }

        /// <summary>
        /// Signs a request to the Breezy API with an OAuth token
        /// </summary>
        private void SignOAuthRequest(HttpClient httpClient, OAuthToken accessToken, HttpMethod method, string endpoint,
                                      IDictionary<string, string> parameters = null)
        {
            var request = OAuthRequest.ForProtectedResource(method.ToString().ToUpperInvariant(),
                                                            _clientKey, _clientSecret,
                                                            accessToken.Token, accessToken.TokenSecret);
            request.Method = method.ToString().ToUpperInvariant();
            request.RequestUrl = _apiUri.AbsoluteUri + endpoint;

            var authHeader = parameters != null
                ? request.GetAuthorizationHeader(parameters)
                : request.GetAuthorizationHeader();

            httpClient.DefaultRequestHeaders.Remove("Authorization");
            httpClient.DefaultRequestHeaders.Add("Authorization", authHeader);
        }
    }
}