using System;

namespace ReMi.DataAccess.Exceptions
{
    public class AccountHasManyDefaultProductsException : ApplicationException
    {
        public AccountHasManyDefaultProductsException(object entityRef)
            : base(FormatMessage(entityRef))
        {
        }

        public AccountHasManyDefaultProductsException(string entityType, object entityRef, Exception innerException)
            : base(FormatMessage(entityRef), innerException)
        {
        }

        private static string FormatMessage(object entityRef)
        {
            return string.Format("Account has multiple default products '{0}'", entityRef);
        }
    }
}
