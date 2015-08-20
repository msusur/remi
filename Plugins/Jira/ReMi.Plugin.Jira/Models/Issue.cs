namespace ReMi.Plugin.Jira.Models
{
    public class Issue
    {
        public int Id { get; set; }
        public string Key { get; set; }

        public IssueFields Fields { get; set; }

        public override string ToString()
        {
            return string.Format("[Id={0}, Key={1}, Fields={2}]", Id, Key, Fields);
        }
    }
}
