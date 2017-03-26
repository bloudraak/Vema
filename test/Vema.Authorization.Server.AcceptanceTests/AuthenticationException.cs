using System;

namespace Vema.Authorization.Server.AcceptanceTests
{
    public class AuthenticationException : Exception
    {
        public string ErrorCode { get; }

        public AuthenticationException(string errorCode)
        {
            ErrorCode = errorCode;
        }

        public AuthenticationException(string errorCode, string message) : base(message)
        {
            ErrorCode = errorCode;
        }

        public AuthenticationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}