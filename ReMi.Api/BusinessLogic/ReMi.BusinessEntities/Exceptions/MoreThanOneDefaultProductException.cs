using System;
using ReMi.BusinessEntities.Auth;

namespace ReMi.BusinessEntities.Exceptions
{
	public class MoreThanOneDefaultProductException : ApplicationException
	{
		public MoreThanOneDefaultProductException(Account account)
            : base(FormatMessage(account))
		{
		}

        public MoreThanOneDefaultProductException(Account account, Exception innerException)
            : base(FormatMessage(account), innerException)
		{
		}

		private static string FormatMessage(Account account)
		{
			return string.Format("More than one default product for account {0}. Contact with Administrator.",  account.FullName);
		}

		
	}
}
