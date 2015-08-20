using System;
using System.Collections.Generic;
using ReMi.Common.Utils;

namespace ReMi.DataEntities.ExecPoll
{
    public class CommandExecution
    {
        #region .ctor

        public CommandExecution()
        {
            CreatedOn = SystemTime.Now;
        }

        #endregion

        #region scalar props

        public int CommandExecutionId { get; set; }

        public Guid ExternalId { get; set; }

        public string Description { get; set; }

        public DateTime CreatedOn { get; set; }

        #endregion

        #region navigational props

        public virtual List<CommandHistory> CommandHistory { get; set; }

        #endregion

        public override string ToString()
        {
            return string.Format("[CommandExecutionId = {0}, ExternalId = {1}, Description = {2}, CreatedOn = {3}]", CommandExecutionId, ExternalId, Description, CreatedOn);
        }
    }
}
