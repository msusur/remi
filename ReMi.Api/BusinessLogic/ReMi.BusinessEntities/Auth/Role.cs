using System;

namespace ReMi.BusinessEntities.Auth
{
    public class Role
    {
        public Guid ExternalId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public override string ToString()
        {
            return string.Format("[Name={0}, Description={1}, ExtrenalId={2}]",
                Name, Description, ExternalId);
        }

        public override bool Equals(object obj)
        {
            var temp = obj as Role;
            return temp != null && string.Equals(temp.Name, Name);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }
}
