
namespace ReMi.Plugin.Go.Entities
{
    public class RevisionInfo
    {
        public string Repository { get; set; }
        public string ModifiedTime { get; set; }
        public string User { get; set; }
        public string Comment { get; set; }
        public string Revision { get; set; }

        public override string ToString()
        {
            return string.Format("[Revision={0}, Repository={1}, ModifiedTime={2}, User={3}, Comment={4}]",
                Revision, Repository, ModifiedTime, User, Comment);
        }
    }
}
