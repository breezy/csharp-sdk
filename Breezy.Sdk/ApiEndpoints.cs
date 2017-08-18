namespace Breezy.Sdk
{
    internal static class ApiEndpoints
    {
        public const string RequestToken = "oauth/request_token";
        public const string SsoAuthorize = "oauth/sso/authorize";
        public const string AccessToken = "oauth/access_token";
        public const string GetUserPrinters = "user/printers/enterprise";
        public const string SubmitDocument = "document";
        public const string UpdateDocumentStatus = "document/{document_id}/status";
    }
}