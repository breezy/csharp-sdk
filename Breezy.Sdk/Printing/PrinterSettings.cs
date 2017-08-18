using Newtonsoft.Json;

namespace Breezy.Sdk.Printing
{
    public class PrinterSettings
    {
        [JsonProperty("color")]
        public OutputColor OutputColor { get; set; }

        [JsonProperty("orientation")]
        public PageOrientation Orientation { get; set; }

        [JsonProperty("duplexing")]
        public Duplexing Duplexing { get; set; }

        [JsonProperty("number_of_copies")]
        public int NumberOfCopies { get; set; }

        [JsonProperty("page_range")]
        public string PagesToPrint { get; set; }

        public PrinterSettings()
        {
            OutputColor = OutputColor.Monochrome;
            Orientation = PageOrientation.Portrait;
            Duplexing = Duplexing.OneSided;
            NumberOfCopies = 1;
        }
    }
}
