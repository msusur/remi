using System;

namespace ReMi.Plugin.Common
{
    public class RestApiRequestException : Exception
    {
        public string ResponseContent { get; private set; }

        public RestApiRequestException(string message, string responseContent, Exception innerException)
            : base(message, innerException)
        {
            ResponseContent = responseContent;
        }
    }
}
