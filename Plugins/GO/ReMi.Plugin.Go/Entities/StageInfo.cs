
namespace ReMi.Plugin.Go.Entities
{
    public class StageInfo
    {
        public string Locator { get; set; }
        public string Status { get; set; }
        public string Name { get; set; }

        public override string ToString()
        {
            return string.Format("[Name={0}, Status={1}, Locator={2}]",
                Name, Status, Locator);
        }
    }
}
