using System;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Queries;

namespace ReMi.Queries.Subscriptions
{
    [Query("Get notification subscriptions for specified account", QueryGroup.Subscriptions)]
    public class GetAccountNotificationSubscriptionsRequest : IQuery
    {
        public QueryContext Context { get; set; }
        public Guid AccountId { get; set; }

        public override string ToString()
        {
            return String.Format("[AccountId={0}, Context={1}]", AccountId, Context);
        }
    }
}
