using System;
using System.Collections.Generic;
using ReMi.Common.Utils;

namespace ReMi.DataAccess.Exceptions.Configuration
{
	public class ProductNotFoundException : ApplicationException
	{
		public ProductNotFoundException(string productName)
			: base(FormatMessage(productName))
		{
		}
        public ProductNotFoundException(IEnumerable<string> productNames)
            : base(FormatMessage(productNames))
        {
        }

		public ProductNotFoundException(string productName, Exception innerException)
			: base(FormatMessage(productName), innerException)
		{
		}

		private static string FormatMessage(string productName)
		{
			return string.Format("Could not find product with name: {0}", productName);
		}
        private static string FormatMessage(IEnumerable<string> productNames)
        {
            return string.Format("Could not find products: {0}", productNames.FormatElements());
        }
    }
}
