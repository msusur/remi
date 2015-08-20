using System;

namespace ReMi.DataAccess.Exceptions
{
    public class ReleaseApproverNotFoundException : EntityNotFoundException
    {
        public ReleaseApproverNotFoundException(Guid accountId) :
            base("ReleaseApprover", accountId)
        {
        }

        public ReleaseApproverNotFoundException(Guid accountId, Exception innerException) :
            base("ReleaseApprover", accountId, innerException)
        {
        }
    }
}
