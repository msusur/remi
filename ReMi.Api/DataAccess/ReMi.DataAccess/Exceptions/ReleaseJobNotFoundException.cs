using System;
using System.Collections.Generic;

namespace ReMi.DataAccess.Exceptions
{
    public class ReleaseJobNotFoundException : EntityNotFoundException
    {
        public ReleaseJobNotFoundException(int jobId, Guid releaseWindowId)
            : base("ReleaseJob", new KeyValuePair<int, Guid>(jobId, releaseWindowId))
        {
        }

        public ReleaseJobNotFoundException(int jobId, Guid releaseWindowId, Exception innerException)
            : base("ReleaseJob", new KeyValuePair<int, Guid>(jobId, releaseWindowId), innerException)
        {
        }
    }
}
