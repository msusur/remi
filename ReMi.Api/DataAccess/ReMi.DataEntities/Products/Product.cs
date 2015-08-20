using ReMi.Common.Constants.ReleaseCalendar;
using ReMi.DataEntities.Auth;
using ReMi.DataEntities.ReleaseCalendar;
using ReMi.DataEntities.ReleasePlan;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using ReMi.DataEntities.Plugins;

namespace ReMi.DataEntities.Products
{
    public class Product
    {
        public int ProductId { get; set; }

        public string Description { get; set; }

        [Index(IsUnique = true)]
        public Guid ExternalId { get; set; }

        public ReleaseTrack ReleaseTrack { get; set; }

        public Boolean ChooseTicketsByDefault { get; set; }

        public int BusinessUnitId { get; set; }

       
        public virtual BusinessUnit BusinessUnit { get; set; }

        public virtual ICollection<ReleaseProduct> ReleaseProducts { get; set; }

        public virtual IList<CheckListQuestionToProduct> CheckListQuestionsToProducts { get; set; }

        public virtual IList<AccountProduct> AccountProducts { get; set; }

        public virtual IList<PluginPackageConfiguration> PluginPackageConfiguration { get; set; }

        public override string ToString()
        {
            return
                string.Format(
                    "[ProductId={0}, Description={1}, ExternalId={2}, ReleaseTrack={3}, ChooseTicketsByDefault={4}, BusinessUnit={5}]", 
                    ProductId, Description, ExternalId, ReleaseTrack, ChooseTicketsByDefault, BusinessUnit);
        }
    }
}
