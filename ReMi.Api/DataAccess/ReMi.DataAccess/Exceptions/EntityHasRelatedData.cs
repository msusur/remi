using System;

namespace ReMi.DataAccess.Exceptions
{
    public class EntityHasRelatedData : ApplicationException
    {
        public EntityHasRelatedData(Type entityType, object entityRef)
            : base(FormatMessage(entityType.Name, entityRef))
        {
        }

        public EntityHasRelatedData(string entityType, object entityRef)
            : base(FormatMessage(entityType, entityRef))
        {
        }

        public EntityHasRelatedData(string entityType, object entityRef, Exception innerException)
            : base(FormatMessage(entityType, entityRef), innerException)
        {
        }

        private static string FormatMessage(string entityType, object entityRef)
        {
            return string.Format("Entity '{0}' with type '{1}' has related data", entityRef, entityType);
        }
    }
}
