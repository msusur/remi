namespace ReMi.BusinessEntities.HelpDesk
{
    public class HelpDeskTaskView
    {
        public string Number { get; set; }
        public string Description { get; set; }
        public string LinkUrl { get; set; }

        public override string ToString()
        {
            return string.Format("[Number = {0}, Description = {1}, LinkUrl = {2}]", 
                Number, Description, LinkUrl);
        }
    }
}
