using Newtonsoft.Json;

namespace Breezy.Sdk.Payloads
{
    internal class SsoAuthorizationMessage
    {
        [JsonProperty("mdm_auth_key")]
        public string MdmAuthKey { get; set; }

        [JsonProperty("client_key")]
        public string ClientKey { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        /// <summary>
        /// Current UTC time as a unix time (number of seconds elapsed since 1/1/1970 00:00 UTC)
        /// </summary>
        [JsonProperty("timestamp")]
        public long Timestamp { get; set; }

        /// <summary>
        /// Base64 representation of HMAC-SHA256 hash of Email+Timestamp with client secret as a key
        /// </summary>
        [JsonProperty("signature")]
        public string Signature { get; set; }
    }
}