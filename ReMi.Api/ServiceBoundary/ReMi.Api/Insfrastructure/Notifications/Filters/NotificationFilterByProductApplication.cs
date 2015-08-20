using System;
using System.Collections.Generic;
using System.Linq;
using ReMi.BusinessEntities.Auth;
using ReMi.Common.Utils;
using ReMi.Common.WebApi.Notifications;

namespace ReMi.Api.Insfrastructure.Notifications.Filters
{
    public class NotificationFilterByProductApplication : INotificationFilterByProductApplication
    {
        public bool Apply(INotificationFilterByProduct filter, Account account, List<Subscription> subscriptions)
        {
            var productsLocal = filter.Products.ToArray();

            var t = subscriptions.SelectMany(o => o.Filters)
                .Where(
                    o =>
                        o.Property.Equals(filter.PropertyName(p => p.Products),
                            StringComparison.InvariantCultureIgnoreCase))
                .ToArray();

            Console.WriteLine(t.ToString());

            var result = null !=
                subscriptions.SelectMany(o => o.Filters)
                    .Where(o => o.Property.Equals(filter.PropertyName(p => p.Products), StringComparison.InvariantCultureIgnoreCase))
                    .FirstOrDefault(o => IsStringProductContainsProductItems(o.Value, productsLocal));

            return result;
        }

        private bool IsStringProductContainsProductItems(string value, IEnumerable<string> valuesForCheck)
        {
            var valuesAsArray = value.Trim(new[] {'[', ']'}).Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries);
            if (!valuesAsArray.Any())
            {
                return false;
            }

            if (valuesForCheck == null)
                return false;

            return valuesAsArray.Any(valuesForCheck.Contains);
        }
    }
}
