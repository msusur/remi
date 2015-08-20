using ReMi.BusinessEntities.ExecPoll;

namespace ReMi.Queries.ExecPoll
{
    public class GetCommandStateResponse
    {
        public CommandStateType State { get; set; }

        public string Details { get; set; }

        public override string ToString()
        {
            return string.Format("[State={0}, Details={1}]", State, Details);
        }
    }
}
