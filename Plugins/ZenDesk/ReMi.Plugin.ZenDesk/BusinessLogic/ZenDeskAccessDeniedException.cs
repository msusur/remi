using System;

namespace ReMi.Plugin.ZenDesk
{
    public class ZenDeskAccessDeniedException : ApplicationException
    {
        public ZenDeskAccessDeniedException(string action)
            : base(FormatMessage(action))
        {

        }
        private static string FormatMessage(string action)
        {
            return string.Format("ZenDesk - Sorry, you don't have permission to perform this action: {0}", action);
        }
    }
}
