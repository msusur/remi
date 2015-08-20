using System;

namespace ReMi.Plugin.Gerrit.Service.Model
{
    public class Repository
    {
        public Guid ExternalId { get; set; }
        public string Name { get; set; }
        public bool StartFromLatest { get; set; }
        public string DefaultFrom { get; set; }
        public string DefaultTo { get; set; }
        public bool IsIncludedByDefault { get; set; }
        public bool IsDisabled { get; set; }

        public override string ToString()
        {
            return string.Format("[ExternalId={0}, Name={1}, StartFromLatest={2}, DefaultFrom={3}, DefaultTo={4}, IsIncludedByDefault={5}, IsDisabled={6}]",
                ExternalId, Name, StartFromLatest, DefaultFrom, DefaultTo, IsIncludedByDefault, IsDisabled);
        }
    }
}
