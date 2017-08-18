using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using Breezy.Sdk.Objects;
using Breezy.Sdk.Payloads;
using Breezy.Sdk.Printing;
using Newtonsoft.Json;

namespace Breezy.Sdk
{
    public partial class BreezyApiClient
    {
        /// <summary>
        /// Creates a document with the Breezy API and gets the file upload/encryption settings back
        /// </summary>
        private DocumentUploadMessage SubmitDocument(int? endpointId, string fileName, string fileType, int fileSize,
                                                     PrinterSettings finishingOptions, OAuthToken userAccessToken)
        {
            using (var http = new HttpClient())
            {
                var message = new DocumentSubmissionMessage
                    {
                        EndpointId = endpointId,
                        File = new FileInfoMessage
                            {
                                FriendlyName = fileName,
                                FileType = fileType,
                                FileSize = fileSize
                            },
                        FinishingOptions = finishingOptions
                    };

                var requestContent = new StringContent(JsonConvert.SerializeObject(message), Encoding.UTF8, "application/json");

                SignOAuthRequest(http, userAccessToken, HttpMethod.Post, ApiEndpoints.SubmitDocument);
                var response = http.PostAsync(_apiUri.AbsoluteUri + ApiEndpoints.SubmitDocument, requestContent).Result;
                var responseBody = response.Content.ReadAsStringAsync().Result;

                if (response.StatusCode != HttpStatusCode.OK)
                    throw new BreezyApiException(
                        String.Format("Could not submit document.\r\nResponse status code: {0}.\r\nResponse: {1}",
                                      response.StatusCode, responseBody));

                return JsonConvert.DeserializeObject<DocumentUploadMessage>(responseBody);
            }
        }

        /// <summary>
        /// Encrypts a stream with a random symmetric key
        /// </summary>
        /// <param name="unencryptedStream">stream to encrypt</param>
        /// <param name="publicKeyModulus">modulus of the RSA key to encrypt the symmetric key with</param>
        /// <param name="publicKeyExponent">exponent of the RSA key to encrypt the symmetric key with</param>
        /// <param name="symmetricEncryptionParameters">returns encrypted symmetric key and IV</param>
        /// <returns>encrypted stream</returns>
        private Stream EncryptStream(Stream unencryptedStream, string publicKeyModulus, string publicKeyExponent,
                                     out SymmetricEncryptionParameters symmetricEncryptionParameters)
        {
            // generate a symmetric key
            var aesProvider = new RijndaelManaged();
            byte[] encodedSymmetricKey = Encoding.ASCII.GetBytes(Convert.ToBase64String(aesProvider.Key));
            byte[] encodedSymmetricIV = Encoding.ASCII.GetBytes(Convert.ToBase64String(aesProvider.IV));

            // encrypt the stream
            var encryptedStream = new MemoryStream();

            using (var aesEncryptor = aesProvider.CreateEncryptor())
            using (var base64Enc = new CryptoStream(unencryptedStream, new ToBase64Transform(), CryptoStreamMode.Read))
            using (var encrypted = new CryptoStream(base64Enc, aesEncryptor, CryptoStreamMode.Read))
            {
                encrypted.CopyTo(encryptedStream);
                encryptedStream.Position = 0;
            }

            // create an RSA key
            var parameters = new RSAParameters
                {
                    Modulus = Util.DecodeHexString(publicKeyModulus),
                    Exponent = Util.DecodeHexString(publicKeyExponent)
                };

            var encryptionProvider = new RSACryptoServiceProvider();
            encryptionProvider.ImportParameters(parameters);

            // encrypt the symmetric key with the RSA key
            byte[] encryptedSymmetricKey = encryptionProvider.Encrypt(encodedSymmetricKey, false);
            byte[] encryptedSymmetricIV = encryptionProvider.Encrypt(encodedSymmetricIV, false);

            symmetricEncryptionParameters = new SymmetricEncryptionParameters
                {
                    Key = BitConverter.ToString(encryptedSymmetricKey).Replace("-", ""),
                    IV = BitConverter.ToString(encryptedSymmetricIV).Replace("-", "")
                };

            return encryptedStream;
        }

        /// <summary>
        /// Uploads a stream to the specified URL
        /// </summary>
        private void UploadStreamToBuffer(Stream stream, string uploadUrl)
        {
            using (var http = new HttpClient())
            {
                var response = http.PutAsync(uploadUrl, new StreamContent(stream)).Result;
                if (response.StatusCode != HttpStatusCode.OK)
                    throw new BreezyApiException(
                        String.Format("Could not upload file stream.\r\nResponse status code: {0}.\r\nResponse: {1}",
                                      response.StatusCode, response.Content.ReadAsStringAsync().Result));
            }
        }

        /// <summary>
        /// Submits information about the uploaded document to the API
        /// </summary>
        private void UpdateDocumentStatusAfterFileUpload(int documentId, SymmetricEncryptionParameters encryptionParameters,
                                                         string fileName, string fileType, int fileSize,
                                                         PrinterSettings finishingOptions, OAuthToken userAccessToken)
        {
            using (var http = new HttpClient())
            {
                var message = new DocumentStatusMessage
                    {
                        IsSuccess = true,
                        Status = DocumentStatus.ClientUpload,
                        ClientUploadDocument = new FileUploadMessage
                            {
                                EncryptedSymmetricKey = encryptionParameters.Key,
                                EncryptedSymmetricIV = encryptionParameters.IV,
                                File = new FileInfoMessage
                                    {
                                        FriendlyName = fileName,
                                        FileType = fileType,
                                        FileSize = fileSize
                                    },
                                PrintOptions = finishingOptions
                            }
                    };

                var endpoint = ApiEndpoints.UpdateDocumentStatus.Replace("{document_id}", documentId.ToString(CultureInfo.InvariantCulture));
                var requestContent = new StringContent(JsonConvert.SerializeObject(message), Encoding.UTF8, "application/json");

                SignOAuthRequest(http, userAccessToken, HttpMethod.Post, endpoint);
                var response = http.PostAsync(_apiUri.AbsoluteUri + endpoint, requestContent).Result;

                if (response.StatusCode != HttpStatusCode.OK)
                    throw new BreezyApiException(
                        String.Format("Could not update document status.\r\nResponse status code: {0}.\r\nResponse: {1}",
                                      response.StatusCode, response.Content.ReadAsStringAsync().Result));
            }
        }
    }
}