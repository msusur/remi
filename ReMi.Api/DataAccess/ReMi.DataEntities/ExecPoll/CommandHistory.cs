using System;
using ReMi.Common.Utils;

namespace ReMi.DataEntities.ExecPoll
{
    public class CommandHistory
    {
        #region .ctor

        public CommandHistory()
        {
            CreatedOn = SystemTime.Now;
        }

        #endregion

        #region scalar props

        public int CommandHistoryId { get; set; }

        public int CommandExecutionId { get; set; }

        public CommandStateType State { get; set; }

        public DateTime CreatedOn { get; set; }

        public string Details { get; set; }

        #endregion

        #region navigational props

        public virtual CommandExecution Command { get; set; }

        #endregion

        public override string ToString()
        {
            return string.Format("[CommandExecutionId={0}, State={1}, CreatedOn={2}, CommandHistoryId={3}, Details={4}]",
                CommandExecutionId, State, CreatedOn, CommandHistoryId, Details);
        }
    }
}
