using System;

namespace ReMi.DataAccess.Exceptions.Configuration
{
    public class ProductAlreadyExistsException : ApplicationException
    {
        public ProductAlreadyExistsException(String product)
            : base(FormatMessage(product))
        {
        }

        public ProductAlreadyExistsException(String product, Exception innerException)
            : base(FormatMessage(product), innerException)
        {
        }

        private static string FormatMessage(String product)
        {
            return string.Format("Product already exists: {0}", product);
        }
    }
}
