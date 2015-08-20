namespace ReMi.BusinessEntities.ExecPoll
{
    public class CommandState
    {
        public CommandStateType StateType { get; set; }

        public string Details { get; set; }

        public override string ToString()
        {
            return string.Format("[StateType={0}, Details={1}]", 
                StateType, Details);
        }
    }
}
