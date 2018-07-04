using Newtonsoft.Json;


namespace Breezy.Sdk.Payloads
{
    class AuthorizationMessage
    {
        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }

        //These are optional parameters
        [JsonProperty("client_id", NullValueHandling = NullValueHandling.Ignore)]
        public int? ClientId { get; set; }

        [JsonProperty("cluster_id", NullValueHandling = NullValueHandling.Ignore)]
        public int? ClusterId { get; set; }


    }
}
