using Breezy.Sdk.Printing;
using Newtonsoft.Json;

namespace Breezy.Sdk.Payloads
{
    internal class DocumentSubmissionMessage
    {
        [JsonProperty("endpoint_id")]
        public int? EndpointId { get; set; }

        [JsonProperty("file")]
        public FileInfoMessage File { get; set; }

        [JsonProperty("finishing_options")]
        public PrinterSettings FinishingOptions { get; set; }
    }
}