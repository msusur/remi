using System;

namespace ReMi.DataAccess.Exceptions
{
    public class EntityNotFoundException : ApplicationException
    {
        public string EntityType { get; set; }

        public EntityNotFoundException(Type entityType, object entityRef)
            : base(FormatMessage(entityType.Name, entityRef))
        {
            EntityType = entityType.Name;
        }

        public EntityNotFoundException(string entityType, object entityRef)
            : base(FormatMessage(entityType, entityRef))
        {
            EntityType = entityType;
        }

        public EntityNotFoundException(string entityType, object entityRef, Exception innerException)
            : base(FormatMessage(entityType, entityRef), innerException)
        {
            EntityType = entityType;
        }

        private static string FormatMessage(string entityType, object entityRef)
        {
            return string.Format("Could not find entity '{0}' of type '{1}'", entityRef, entityType);
        }
    }
}
