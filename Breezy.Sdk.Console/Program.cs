using System.IO;
using System.Linq;

namespace Breezy.Sdk.Console
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            const string apiClientKey = "";
            const string apiClientSecret = "";
            const string mdmAuthKey = "";
            const string userEmail = "";
            const string printerName = "";
            const string filePath = "";

            // create a Breezy API client and authorize the user
            var breezyApiClient = new BreezyApiClient(apiClientKey, apiClientSecret);
            var userAccessToken = breezyApiClient.Authorize(mdmAuthKey, userEmail);

            // get a printer list
            var printers = breezyApiClient.GetPrinters(userAccessToken);

            // find a printer by name
            var printer = printers.FirstOrDefault(x => x.DisplayName == printerName);
            if (printer == null)
            {
                System.Console.WriteLine("Printer {0} is not available.", printerName);
                return;
            }

            // print the file
            var documentId = breezyApiClient.Print(Path.GetFileName(filePath), filePath, printer.Id, userAccessToken);
            System.Console.WriteLine("Document has been printed. Document Id: " + documentId);
        }
    }
}