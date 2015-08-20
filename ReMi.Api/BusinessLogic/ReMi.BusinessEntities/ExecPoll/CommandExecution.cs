using System;

namespace ReMi.BusinessEntities.ExecPoll
{
    public class CommandExecution
    {
        #region props

        public string Description { get; set; }

        public CommandStateType State { get; set; }

        public DateTime CreatedOn { get; set; }

        public Guid ExternalId { get; set; }

        public string Details { get; set; }

        #endregion

        public override string ToString()
        {
            return string.Format("[Description={0}, State={1}, CreatedOn={2}, ExternalId={3}, Details={4}]",
                Description, State, CreatedOn, ExternalId, Details);
        }

    }
}
