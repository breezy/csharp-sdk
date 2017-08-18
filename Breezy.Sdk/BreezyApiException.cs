using System;

namespace Breezy.Sdk
{
    public class BreezyApiException : Exception
    {
        public BreezyApiException(string message) : base(message)
        {
        }
    }
}