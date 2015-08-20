namespace ReMi.Plugin.Go.Entities
{
    public class JobInfo
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Agent { get; set; }
        public string BuildLocator { get; set; }
        public string CurrentStatus { get; set; }
        public string Is_Completed { get; set; }
        public string Result { get; set; }

        public string Build_Completed_Date { get; set; }
        public string Build_Scheduled_Date { get; set; }

        public override string ToString()
        {
            return string.Format("[id={0}, Name={1}, Agent={2}, BuildLocator={3}, CurrentStatus={4}, Is_Completed={5}, Result={6}, Build_Completed_Date={7}, Build_Scheduled_Date={8}]",
                Id, Name, Agent, BuildLocator, CurrentStatus, Is_Completed, Result, Build_Completed_Date, Build_Scheduled_Date);
        }
    }
}
