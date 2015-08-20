using System;

namespace ReMi.DataAccess.Exceptions
{
    public class QueryNotFoundException : EntityNotFoundException
    {
        public QueryNotFoundException(int queryId)
            : base("Query", queryId)
        {
        }

        public QueryNotFoundException(int queryId, Exception innerException)
            : base("Query", queryId, innerException)
        {
        }
    }
}
