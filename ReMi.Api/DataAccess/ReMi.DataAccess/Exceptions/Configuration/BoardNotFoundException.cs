using System;

namespace ReMi.DataAccess.Exceptions.Configuration
{
    public class BoardNotFoundException : ApplicationException
    {
        public BoardNotFoundException(Guid boardId)
            : base(FormatMessage(boardId))
        {
        }

        public BoardNotFoundException(Guid boardId, Exception innerException)
            : base(FormatMessage(boardId), innerException)
        {
        }

        private static string FormatMessage(Guid boardId)
        {
            return string.Format("JQL query not found for board: {0}", boardId);
        }
    }
}
