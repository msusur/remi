namespace ReMi.DataEntities.ExecPoll
{
    public class CommandStateTypeDescription
	{
		#region scalar props

        public int CommandStateTypeId { get; set; }

		public string Description { get; set; }

		#endregion

		public override string ToString()
		{
            return string.Format("[CommandStateTypeId = {0}, Description = {1}]", CommandStateTypeId, Description);
		}
	}
}
