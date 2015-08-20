using System;

namespace ReMi.DataAccess.Exceptions
{
    public class QueryDuplicationException : EntityAlreadyExistsException
    {
        public QueryDuplicationException(Guid externalId)
            : base("Query", externalId)
        {
        }

        public QueryDuplicationException(Guid externalId, Exception innerException)
            : base("Query", externalId, innerException)
        {
        }
    }
}
