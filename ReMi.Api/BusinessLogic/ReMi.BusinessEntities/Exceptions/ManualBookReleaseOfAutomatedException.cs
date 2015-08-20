using System;
using System.Collections.Generic;
using ReMi.Common.Utils;

namespace ReMi.BusinessEntities.Exceptions
{
	public class ManualBookReleaseOfAutomatedException : ApplicationException
	{
		public ManualBookReleaseOfAutomatedException(string product)
            : base(FormatMessage(product))
		{
		}

        public ManualBookReleaseOfAutomatedException(IEnumerable<string> products)
            : base(FormatMessage(products))
        {
        }

        public ManualBookReleaseOfAutomatedException(string product, Exception innerException)
            : base(FormatMessage(product), innerException)
		{
		}

        private static string FormatMessage(string product)
		{
            return string.Format("Cannot manually create release which is marked as Automated [{0}].", product);
		}

        private static string FormatMessage(IEnumerable<string> products)
        {
            return string.Format("Cannot manually create release which is marked as Automated {0}.", products.FormatElements());
        }
		
	}
}
