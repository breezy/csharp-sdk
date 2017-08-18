using Newtonsoft.Json;

namespace Breezy.Sdk
{
    public class Printer
    {
        [JsonProperty("endpoint_id")]
        public int Id { get; set; }

        [JsonProperty("display_name")]
        public string DisplayName { get; set; }
    }
}