using System;

namespace ReMi.Contracts.Plugins.Data.Exceptions
{
    public class FailedToRetrieveSourceControlChangesException : ApplicationException
    {
        public FailedToRetrieveSourceControlChangesException(string message)
            : base(message)
        {

        }

        public FailedToRetrieveSourceControlChangesException(string message, Exception innerException)
            : base(message, innerException)
        {

        }
    }
}
