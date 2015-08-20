using System;
using System.Collections.Generic;
using ReMi.BusinessEntities.Auth;
using ReMi.Common.Utils;

namespace ReMi.BusinessEntities.ProductRequests
{
    public class ProductRequestGroup
    {
        public Guid ExternalId { get; set; }
        public string Name { get; set; }

        public Guid ProductRequestTypeId { get; set; }

        public IEnumerable<ProductRequestTask> RequestTasks { get; set; }
        public IEnumerable<Account> Assignees { get; set; }

        public override string ToString()
        {
            return string.Format("[Name={0}, ExternalId={1}, ProductRequestTypeId={2}, RequestTasks={3}, Assignees={4}]",
                Name, ExternalId, ProductRequestTypeId, RequestTasks.FormatElements(), Assignees.FormatElements());
        }
    }
}
