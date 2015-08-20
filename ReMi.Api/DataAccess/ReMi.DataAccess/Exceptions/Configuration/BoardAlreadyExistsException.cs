using System;

namespace ReMi.DataAccess.Exceptions.Configuration
{
    public class BoardAlreadyExistsException : ApplicationException
    {
        public BoardAlreadyExistsException(string query)
            : base(FormatMessage(query))
        {
        }

        public BoardAlreadyExistsException(string query, Exception innerException)
            : base(FormatMessage(query), innerException)
        {
        }

        private static string FormatMessage(string query)
        {
            return string.Format("Board already exists for query: {0}", query);
        }
    }
}
