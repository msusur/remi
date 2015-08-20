using System;

namespace ReMi.BusinessEntities.Exceptions
{
    public class MoreThanOnePackageAssignToReleaseException : Exception
    {
        public MoreThanOnePackageAssignToReleaseException(Guid releaseWindowId)
            : base(FormatMessage(releaseWindowId))
        {
        }

        public MoreThanOnePackageAssignToReleaseException(Guid releaseWindowId, Exception innerException)
            : base(FormatMessage(releaseWindowId), innerException)
        {
        }

        private static string FormatMessage(Guid releaseWindowId)
        {
            return string.Format("This release type should not have more than one package associated. ReleaseWindowId: {0}", releaseWindowId);
        }
    }
}
