using System;
using System.Collections.Generic;
using System.Linq;

namespace ReMi.Api.Insfrastructure.Notifications
{
    public class ClientRegistration
    {
        public string ConnectionId { get; set; }
        public string Token { get; set; }
        public Guid SessionId { get; set; }
        public string UserName { get; set; }

        public Subscriptions Subscriptions { get; private set; }

        public override string ToString()
        {
            return string.Format("[ConnectionId={0}, Token={1}, SessionId={2}, UserName={3}, Subscriptions={4}]",
                ConnectionId, Token, SessionId, UserName, Subscriptions);
        }

        public ClientRegistration()
        {
            Subscriptions = new Subscriptions();
        }
    }

    public class Subscriptions : List<Subscription>
    {
        public override string ToString()
        {
            return string.Format("[Events={0}]", string.Join(", ", this.ToList().Select(o => o.EventName).ToArray()));
        }
    }

    public class Subscription
    {
        public string EventName { get; set; }
        public SubscriptionFilters Filters { get; set; }

        public override string ToString()
        {
            return string.Format("[EventName={0}, Filters={1}]", EventName, Filters);
        }
    }

    public class SubscriptionFilters : List<SubscriptionFilter>
    {
        public override string ToString()
        {
            return string.Format("[{0}]", string.Join(", ", this.ToList().Select(o => string.Format("({0}={1})", o.Property, o.Value)).ToArray()));
        }
    }

    public class SubscriptionFilter
    {
        public string Property { get; set; }
        public string Value { get; set; }

        public override string ToString()
        {
            return string.Format("[Property={0}, Value={1}]", Property, Value);
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            var target = obj as SubscriptionFilter;
            if (target == null) return false;

            return target.Property.Equals(Property) && target.Value.Equals(Value);
        }

        public override int GetHashCode()
        {
            int hashProperty = string.IsNullOrWhiteSpace(Property) ? 0 : Property.GetHashCode();
            int hashValue = string.IsNullOrWhiteSpace(Value) ? 0 : Value.GetHashCode();

            return hashProperty ^ hashValue;
        }
    }

    public class ProductComparer : IEqualityComparer<SubscriptionFilter>
    {
        public bool Equals(SubscriptionFilter x, SubscriptionFilter y)
        {
            if (ReferenceEquals(x, y)) return true;

            return x != null && y != null && x.Property.Equals(y.Property) && x.Value.Equals(y.Value);
        }

        public int GetHashCode(SubscriptionFilter obj)
        {
            int hashProperty = string.IsNullOrWhiteSpace(obj.Property) ? 0 : obj.Property.GetHashCode();
            int hashValue = string.IsNullOrWhiteSpace(obj.Value) ? 0 : obj.Value.GetHashCode();

            return hashProperty ^ hashValue;
        }
    }
}
