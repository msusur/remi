using System;

namespace ReMi.DataAccess.Exceptions
{
    public class ProductShouldBeAssignedException : ApplicationException
    {
        public ProductShouldBeAssignedException(Guid releaseWindowId)
            : base(FormatMessage(releaseWindowId))
        {
        }

        public ProductShouldBeAssignedException(Guid releaseWindowId, Exception innerException)
            : base(FormatMessage(releaseWindowId), innerException)
        {
        }

        private static string FormatMessage(Guid releaseWindowId)
        {
            return string.Format("Release '{0}' should be assigned with at least one product", releaseWindowId);
        }
    }
}
