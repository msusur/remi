using System;

namespace ReMi.BusinessEntities.Exceptions
{
    public class TeamMembersNotFoundException : ApplicationException
    {
        public TeamMembersNotFoundException(String product)
            : base(FormatMessage(product))
		{
		}

        public TeamMembersNotFoundException(String product, Exception innerException)
            : base(FormatMessage(product), innerException)
		{
		}

        private static string FormatMessage(String product)
		{
            return string.Format("Missing team members for product={0}.", product);
		}
    }
}
