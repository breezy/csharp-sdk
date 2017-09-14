# csharp-sdk

### Prerequisites
- .NET Framework 4.5
- Visual Studio 2012 or higher
- An [mdmauthkey](https://dashboard.breezy.com/settings) (set via Settings-->Organization Settings-->MDM Authorization Key).
- A Client Key and a Client Secret, which your Breezy contact will provide.

### Third-party dependencies
- Json.NET (https://www.nuget.org/packages/Newtonsoft.Json/)
- OAuth (https://www.nuget.org/packages/OAuth/)

### Notes 
- This SDK is designed to be used as part of the backend of an application that requires user authentication. 
- The sample code assumes that you want to offer a frictionless authentication experience to your end user, and therefore do not want to prompt them for Breezy login credentials.  
- Because no user interaction is required to authenticate the user, you should be aware that any user on whose behalf your application submits a document will be automatically added to your organization. In other words, you do not need to invite users or manually provision them via the Dashboard. Instead, *your application will be treated as a trusted provider of user authentication for your organization's Breezy account*, and Breezy will assume that any users you have allowed to access the print functionality in your application are in fact authorized by you to print to your organization's printers via the Breezy infrastructure.

### Usage
The libraries in the SDK allow your application, on behalf of the user, to obtain a list of printers available to that user, and to submit a job for printing to any of those printers.  For access to more advanced functionality such as print job characteristics, job tracking, auditing etc., just ask your Breezy contact.


### Sample Code
- `Breezy.Sdk.Console/Program.cs` provides a simple demo.  
- You will need to provide the appropriate values for `apiClientKey`, `apiClientSecret`, `mdmAuthKey`, `userEmail`, `printerName`, and `filePath`, but no other changes are required to run the simple console application.  
- You can get a list of `printerName`s via the `breezyApiClient.GetPrinters(userAccessToken)` call. The call returns an array of `Breezy.Sdk.Printer` objects, which you can inspect to find the names of the printers available to the user.

### Relevant function calls in the sample code:
```csharp
var breezyApiClient = new BreezyApiClient(apiClientKey, apiClientSecret);

// authorize a user via SSO with an mdmAuthKey
var userAccessToken = breezyApiClient.Authorize(mdmAuthKey, userEmail);

// get available printers
var printers = breezyApiClient.GetPrinters(userAccessToken);

// print a file
var documentId = breezyApiClient.Print(Path.GetFileName(filePath), filePath, printerId, userAccessToken);
```

### Usage from PowerShell
```posh
[System.Reflection.Assembly]::LoadFile("C:\bz-pkgs\Breezy.Sdk\Breezy.Sdk.dll")

$apiClientKey = "..."
$apiClientSecret = "..."
$apiURL = "https://api.breezy.com/"
$apiClient = New-Object Breezy.Sdk.BreezyApiClient -argumentlist $apiClientKey, $apiClientSecret, $apiURL

$mdmAuthKey = "..."
$userEmail = "user@example.org"
$userAccessToken = $apiClient.Authorize($mdmAuthKey, $userEmail)

$endpoint = "..."
$method = "GET"
$response = $apiClient.MakeApiRequest($userAccessToken, $endpoint, $method)
```