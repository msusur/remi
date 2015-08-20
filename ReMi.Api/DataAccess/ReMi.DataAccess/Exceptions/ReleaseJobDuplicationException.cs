using System;
using System.Collections.Generic;

namespace ReMi.DataAccess.Exceptions
{
    public class ReleaseJobDuplicationException : EntityAlreadyExistsException
    {
        public ReleaseJobDuplicationException(int jobId, Guid releaseWindowId)
            : base("ReleaseJob", new KeyValuePair<int, Guid>(jobId, releaseWindowId))
        {
        }

        public ReleaseJobDuplicationException(int jobId, Guid releaseWindowId, Exception innerException)
            : base("ReleaseJob", new KeyValuePair<int, Guid>(jobId, releaseWindowId), innerException)
        {
        }
    }
}
