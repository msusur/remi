using System;

namespace ReMi.DataAccess.Exceptions
{
    public class ReleaseApproverDuplicationException : EntityAlreadyExistsException
    {
        public ReleaseApproverDuplicationException(Guid releaseApproverId) :
            base("ReleaseApprover", releaseApproverId)
        {
        }

        public ReleaseApproverDuplicationException(Guid releaseApproverId, Exception innerException) :
            base("ReleaseApprover", releaseApproverId, innerException)
        {
        }
    }
}
