using System.Collections.Generic;
using Newtonsoft.Json;

namespace Breezy.Sdk.Payloads
{
    internal class PrinterListMessage
    {
        [JsonProperty("printers")]
        public List<Printer> Printers { get; set; }
    }
}