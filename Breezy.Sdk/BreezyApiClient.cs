using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using Breezy.Sdk.Objects;
using Breezy.Sdk.Payloads;
using Breezy.Sdk.Printing;
using Newtonsoft.Json;

namespace Breezy.Sdk
{
    /// <summary>
    /// Operations with the Breezy API
    /// </summary>
    public partial class BreezyApiClient
    {
        private readonly string _clientKey;
        private readonly string _clientSecret;
        private readonly Uri _apiUri;

        public BreezyApiClient(string clientKey, string clientSecret, string apiUrl)
        {
            if (clientKey == null) throw new ArgumentNullException("clientKey");
            if (clientSecret == null) throw new ArgumentNullException("clientSecret");
            if (apiUrl == null) throw new ArgumentNullException("apiUrl");

            _clientKey = clientKey;
            _clientSecret = clientSecret;
            _apiUri = new Uri(apiUrl);
        }

        public BreezyApiClient(string clientKey, string clientSecret)
            : this(clientKey, clientSecret, "https://api.breezy.com/")
        {
        }

        /// <summary>
        /// Authorizes the individual Breezy user
        /// </summary>
        /// <param name="mdmAuthKey">MDM Auth Key of the Breezy account</param>
        /// <param name="userEmail">email of the user being authorized</param>
        /// <returns>OAuth access token to use for subsequent calls to the API on behalf of the authorized user</returns>
        public OAuthToken Authorize(string mdmAuthKey, string userEmail)
        {
            if (mdmAuthKey == null) throw new ArgumentNullException("mdmAuthKey");
            if (userEmail == null) throw new ArgumentNullException("userEmail");

            var requestToken = GetRequestToken();
            var oauthVerifier = GetOAuthVerifierWithMdmAuthKey(mdmAuthKey, userEmail, requestToken);
            var accessToken = GetAccessToken(requestToken, oauthVerifier);

            return accessToken;
        }

        /// <summary>
        /// Makes a request to the API endpoint
        /// </summary>
        /// <param name="userAccessToken">OAuth token which the user has been authorized with</param>
        /// <param name="endpoint">API endpoint</param>
        /// <param name="httpMethod">HTTP method of the request</param>
        /// <param name="payload">JSON payload for POST and PUT requests</param>
        /// <returns>content of the response</returns>
        public string MakeApiRequest(OAuthToken userAccessToken, string endpoint, HttpMethod httpMethod,
                                     string payload = null)
        {
            if (userAccessToken == null) throw new ArgumentNullException("userAccessToken");
            if (endpoint == null) throw new ArgumentNullException("endpoint");
            if ((httpMethod == HttpMethod.Post || httpMethod == HttpMethod.Put) && payload == null)
                throw new ArgumentException("Payload cannot be null for POST or PUT requests.");

            using (var http = new HttpClient())
            {
                SignOAuthRequest(http, userAccessToken, httpMethod, endpoint);

                HttpResponseMessage response;
                if (httpMethod == HttpMethod.Get)
                {
                    response = http.GetAsync(_apiUri.AbsoluteUri + endpoint).Result;
                }
                else if (httpMethod == HttpMethod.Post)
                {
                    response = http.PostAsync(_apiUri.AbsoluteUri + endpoint,
                                              new StringContent(payload, Encoding.UTF8, "application/json")).Result;
                }
                else if (httpMethod == HttpMethod.Put)
                {
                    response = http.PutAsync(_apiUri.AbsoluteUri + endpoint,
                                             new StringContent(payload, Encoding.UTF8, "application/json")).Result;
                }
                else if (httpMethod == HttpMethod.Delete)
                {
                    response = http.DeleteAsync(_apiUri.AbsoluteUri + endpoint).Result;
                }
                else
                {
                    throw new NotSupportedException(String.Format("HTTP method {0} not supported.", httpMethod));
                }

                var responseBody = response.Content.ReadAsStringAsync().Result;

                if (response.StatusCode != HttpStatusCode.OK)
                    throw new BreezyApiException(
                        String.Format(
                            "API request failed.\r\nEndpoint: {0}\r\nResponse status code: {1}.\r\nResponse: {2}",
                            endpoint, response.StatusCode, responseBody));

                return responseBody;
            }
        }

        /// <summary>
        /// Returns the list of the printers available for a Breezy user
        /// </summary>
        /// <param name="userAccessToken">OAuth token which the user has been authorized with</param>
        public List<Printer> GetPrinters(OAuthToken userAccessToken)
        {
            var responseBody = MakeApiRequest(userAccessToken, ApiEndpoints.GetUserPrinters, HttpMethod.Get);
            var printerList = JsonConvert.DeserializeObject<PrinterListMessage>(responseBody);
            return printerList.Printers;
        }

        /// <summary>
        /// Prints a document
        /// </summary>
        /// <param name="documentName">user friendly document name with a file extension</param>
        /// <param name="filePath">path to a file to print</param>
        /// <param name="printerId">id of a printer</param>
        /// <param name="userAccessToken">OAuth token which the user has been authorized with</param>
        /// <returns>id of a printed document</returns>
        public int Print(string documentName, string filePath, int printerId, OAuthToken userAccessToken)
        {
            if (filePath == null) throw new ArgumentNullException("filePath");

            if (!File.Exists(filePath))
                throw new ArgumentException(String.Format("File {0} not found.", filePath));

            using (var fs = File.OpenRead(filePath))
            {
                return Print(documentName, fs, printerId, userAccessToken);
            }
        }

        /// <summary>
        /// Prints a document
        /// </summary>
        /// <param name="documentName">user friendly document name with a file extension</param>
        /// <param name="documentStream">data stream to print</param>
        /// <param name="printerId">id of a printer</param>
        /// <param name="userAccessToken">OAuth token the user has been authorized with</param>
        /// <returns>id of a printed document</returns>
        public int Print(string documentName, Stream documentStream, int? printerId, OAuthToken userAccessToken)
        {
            if (documentName == null) throw new ArgumentNullException("documentName");
            if (documentStream == null) throw new ArgumentNullException("documentStream");
            if (userAccessToken == null) throw new ArgumentNullException("userAccessToken");

            var fileType = Path.GetExtension(documentName);
            var fileSize = (int) documentStream.Length;
            var finishingOptions = new PrinterSettings();

            // POST document
            var documentUploadMessage = SubmitDocument(printerId, documentName, fileType, fileSize,
                                                       finishingOptions, userAccessToken);

            // encrypt document
            SymmetricEncryptionParameters symmetricEncryptionParameters;
            using (var encryptedStream = EncryptStream(documentStream, documentUploadMessage.PublicKeyModulus,
                                                       documentUploadMessage.PublicKeyExponent,
                                                       out symmetricEncryptionParameters))
            {
                // upload document
                UploadStreamToBuffer(encryptedStream, documentUploadMessage.UploadUrl);
            }

            // POST document/{document_id}/status
            UpdateDocumentStatusAfterFileUpload(documentUploadMessage.DocumentId, symmetricEncryptionParameters,
                                                documentName, fileType, fileSize, finishingOptions, userAccessToken);

            return documentUploadMessage.DocumentId;
        }
    }
}