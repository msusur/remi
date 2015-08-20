
namespace ReMi.Plugin.ZenDesk.Models
{
    public class Attachment
    {
        public int Id { get; set; }
        public string File_Name { get; set; }
        public string Content_Url { get; set; }
        public int Size { get; set; }

        public override string ToString()
        {
            return string.Format("[Id={0}, File_Name={1}, Content_Url={2}]", Id, File_Name, Content_Url);
        }
    }
}
