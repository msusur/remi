namespace ReMi.Plugin.Jira.Models
{
    public class IssueType
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool SubTask { get; set; }

        public override string ToString()
        {
            return string.Format("[Id={0}, Name={1}, SubTask={2}]", Id, Name, SubTask);
        }
    }
}
