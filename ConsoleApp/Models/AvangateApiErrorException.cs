using System;
using EnsureThat;

namespace ConsoleApp.Models
{
    public class AvangateApiErrorException : Exception
    {
        public AvangateApiErrorException(string message, string method, string url, AvangateApiError error)
            : base(message)
        {
            EnsureArg.IsNotNullOrWhiteSpace(message, nameof(message));
            EnsureArg.IsNotNullOrWhiteSpace(method, nameof(method));
            EnsureArg.IsNotNullOrWhiteSpace(url, nameof(url));
            EnsureArg.IsNotNull(error, nameof(error));

            Method = method;
            Url = url;
            ErrorCode = error.ErrorCode;
            ErrorMessage = error.Message;
        }

        public string Method { get; }
        public string Url { get; }
        public string ErrorMessage { get; }
        public string ErrorCode { get; }
    }
}