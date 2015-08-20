namespace ReMi.DataEntities.Evt
{
    public class EventStateTypeDescription
	{
		#region scalar props

        public int EventStateTypeId { get; set; }

		public string Description { get; set; }

		#endregion

		public override string ToString()
		{
            return string.Format("[EventStateTypeId = {0}, Description = {1}]", 
                EventStateTypeId, Description);
		}
	}
}
