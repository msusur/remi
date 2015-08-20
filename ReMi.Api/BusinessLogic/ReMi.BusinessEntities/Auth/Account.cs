using ReMi.BusinessEntities.Products;
using System;
using System.Collections.Generic;

namespace ReMi.BusinessEntities.Auth
{
    public class Account
    {
        #region props

        public Guid ExternalId { get; set; }

        public string Name { get; set; }

        public string FullName { get; set; }

        public string Email { get; set; }

        public Role Role { get; set; }

        public bool IsBlocked { get; set; }

        public string Description { get; set; }

        public IEnumerable<ProductView> Products { get; set; }

        public DateTime CreatedOn { get; set; }


        #endregion

        public override string ToString()
        {
            return string.Format("[Name={0}, FullName={1}, Email={2}, Role={3}, IsBlocked={4}, Description={5}, CreatedOn={6}, ExternalId={7}]",
                Name, FullName, Email, Role == null ? string.Empty : Role.Name, IsBlocked, Description, CreatedOn, ExternalId);
        }

    }
}
