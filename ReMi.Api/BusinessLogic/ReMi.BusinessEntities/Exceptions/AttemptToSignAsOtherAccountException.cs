using System;

namespace ReMi.BusinessEntities.Exceptions
{
    public class AttemptToSignAsOtherAccountException : ApplicationException
    {
        public AttemptToSignAsOtherAccountException(string userName, Guid accountId)
            : base(FormatMessage(userName, accountId))
        {
        }


        public AttemptToSignAsOtherAccountException(string userName, Guid accountId, Exception innerException)
            : base(FormatMessage(userName, accountId), innerException)
        {
        }

        private static string FormatMessage(string userName, Guid accountId)
        {
            return string.Format("User '{0}' account id is not '{1}'", userName, accountId);
        }
    }}
