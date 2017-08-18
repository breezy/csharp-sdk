using Breezy.Sdk.Printing;
using Newtonsoft.Json;

namespace Breezy.Sdk.Payloads
{
    internal class FileUploadMessage
    {
        [JsonProperty("encrypted_symmetric_key")]
        public string EncryptedSymmetricKey { get; set; }

        [JsonProperty("encrypted_symmetric_iv")]
        public string EncryptedSymmetricIV { get; set; }

        [JsonProperty("print_options")]
        public PrinterSettings PrintOptions { get; set; }

        [JsonProperty("file")]
        public FileInfoMessage File { get; set; }
    }
}