using System;

namespace ReMi.BusinessEntities.ProductRequests
{
    public class ProductRequestRegistrationTask
    {
        public Guid ProductRequestTaskId { get; set; }

        public bool IsCompleted { get; set; }

        public string Comment { get; set; }

        public string LastChangedBy { get; set; }

        public Guid? LastChangedByAccountId { get; set; }

        public DateTime? LastChangedOn { get; set; }

        public override string ToString()
        {
            return string.Format("[ProductRequestTaskId={0}, IsCompleted={1}, LastChangedBy={2}, "+
                "LastChangedByAccountId={3}, LastChangedOn={4}, Comment={5}]",
                ProductRequestTaskId, IsCompleted, LastChangedBy, LastChangedByAccountId, LastChangedOn, Comment);
        }
    }
}
