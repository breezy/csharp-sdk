using Newtonsoft.Json;

namespace Breezy.Sdk.Payloads
{
    internal class FileInfoMessage
    {
        [JsonProperty("friendly_name")]
        public string FriendlyName { get; set; }

        [JsonProperty("file_size")]
        public int FileSize { get; set; }

        [JsonProperty("file_type")]
        public string FileType { get; set; }
    }
}