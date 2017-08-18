using Newtonsoft.Json;

namespace Breezy.Sdk.Payloads
{
    internal class DocumentStatusMessage
    {
        [JsonProperty("is_success")]
        public bool IsSuccess { get; set; }

        [JsonProperty("status")]
        public DocumentStatus Status { get; set; }

        [JsonProperty("client_upload_document")]
        public FileUploadMessage ClientUploadDocument { get; set; }
    }
}