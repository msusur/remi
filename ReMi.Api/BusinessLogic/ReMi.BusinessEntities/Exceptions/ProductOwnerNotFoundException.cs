using System;

namespace ReMi.BusinessEntities.Exceptions
{
    public class ProductOwnerNotFoundException : Exception
    {
        public ProductOwnerNotFoundException(string product)
            : base(FormatMessage(product))
        {
        }

        public ProductOwnerNotFoundException(string product, Exception innerException)
            : base(FormatMessage(product), innerException)
        {
        }

        private static string FormatMessage(string product)
        {
            return String.Format("Product owner cound't be found. Product: {0}", product);
        }
    }
}
