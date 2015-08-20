using System;

namespace ReMi.DataAccess.Exceptions
{
    public class AccountHasNoDefaultProductsException : ApplicationException
    {
        public AccountHasNoDefaultProductsException(object entityRef)
            : base(FormatMessage(entityRef))
        {
        }

        public AccountHasNoDefaultProductsException(string entityType, object entityRef, Exception innerException)
            : base(FormatMessage(entityRef), innerException)
        {
        }

        private static string FormatMessage(object entityRef)
        {
            return string.Format("Account has no default product '{0}'", entityRef);
        }
    }
}
