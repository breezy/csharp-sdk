# csharp-sdk

### Prerequisites
- .NET Framework 4.5
- Visual Studio 2012 or higher

### Third-party dependencies
- Json.NET (https://www.nuget.org/packages/Newtonsoft.Json/)
- OAuth (https://www.nuget.org/packages/OAuth/)

### Usage

Breezy.Sdk.Console/Program.cs is a good place to start

```csharp
var breezyApiClient = new BreezyApiClient(apiClientKey, apiClientSecret);

// authorize a user
var userAccessToken = breezyApiClient.Authorize(mdmAuthKey, userEmail);

// get available printers
var printers = breezyApiClient.GetPrinters(userAccessToken);

// print a file
var documentId = breezyApiClient.Print(Path.GetFileName(filePath), filePath, printerId, userAccessToken);
```