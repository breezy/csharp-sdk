using Newtonsoft.Json;

namespace Breezy.Sdk.Payloads
{
    internal class DocumentUploadMessage
    {
        [JsonProperty("document_id")]
        public int DocumentId { get; set; }

        [JsonProperty("public_key_modulus")]
        public string PublicKeyModulus { get; set; }

        [JsonProperty("public_key_exponent")]
        public string PublicKeyExponent { get; set; }

        [JsonProperty("upload_url")]
        public string UploadUrl { get; set; }
    }
}