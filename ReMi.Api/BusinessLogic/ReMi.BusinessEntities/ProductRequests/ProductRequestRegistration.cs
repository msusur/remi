using System;
using System.Collections.Generic;
using ReMi.Common.Utils;

namespace ReMi.BusinessEntities.ProductRequests
{
    public class ProductRequestRegistration
    {
        public Guid ExternalId { get; set; }

        public string Status { get; set; }

        public string Description { get; set; }

        public Guid ProductRequestTypeId { get; set; }

        public string ProductRequestType { get; set; }

        public string CreatedBy { get; set; }

        public Guid CreatedByAccountId { get; set; }

        public DateTime CreatedOn { get; set; }

        public IEnumerable<ProductRequestRegistrationTask> Tasks { get; set; }

        public override string ToString()
        {
            return string.Format("[ProductRequestTypeId={0}, Status={1}, ProductRequestType={2}, " +
                "Description={3}, CreatedBy={4}, CreatedByAccountId={5}, CreatedOn={6}, Tasks={7}]",
                ProductRequestTypeId, Status, ProductRequestType,
                Description, CreatedBy, CreatedByAccountId, CreatedOn, Tasks.FormatElements());
        }
    }
}
