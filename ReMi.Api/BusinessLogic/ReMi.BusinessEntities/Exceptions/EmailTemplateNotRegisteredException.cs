using System;

namespace ReMi.BusinessEntities.Exceptions
{
    public class EmailTemplateNotRegisteredException : ApplicationException
    {
        public EmailTemplateNotRegisteredException(string key)
            : base(FormatMessage(key))
        {
        }

        public EmailTemplateNotRegisteredException(string key, Exception innerException)
            : base(FormatMessage(key), innerException)
        {
        }

        private static string FormatMessage(string key)
        {
            return string.Format("Email template was not registered. Key={0}.", key);
        }


    }
}
