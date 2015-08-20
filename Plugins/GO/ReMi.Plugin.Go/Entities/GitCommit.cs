namespace ReMi.Plugin.Go.Entities
{
    public class GitCommit
    {
        public string ModifiedTime { get; set; }
        public string User { get; set; }
        public string Comment { get; set; }
        public string Revision { get; set; }
        public string Repository { get; set; }
        public string Name { get; set; }

        public override string ToString()
        {
            return string.Format("[Name={0}, Revision={1}, ModifiedTime={2}, User={3}, Comment={4}, Repository={5}]",
                Name, Revision, ModifiedTime, User, Comment, Repository);
        }
    }
}
