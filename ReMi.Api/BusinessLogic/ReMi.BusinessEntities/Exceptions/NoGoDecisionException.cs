using System;

namespace ReMi.BusinessEntities.Exceptions
{
    public class NoGoDecisionException : ApplicationException
    {
        public NoGoDecisionException(string product)
            : base(FormatMessage(product))
        {
        }

        public NoGoDecisionException(string product, Exception innerException)
            : base(FormatMessage(product), innerException)
        {
        }

        private static string FormatMessage(string product)
        {
            return string.Format("Product {0} cannot be released, QA check did not pass.", product);
        }
    }
}
