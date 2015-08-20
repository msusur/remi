using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Common.Logging;
using ReMi.Api.Insfrastructure.Notifications.Filters;
using ReMi.Api.Insfrastructure.Security;
using ReMi.BusinessEntities.Auth;
using ReMi.BusinessLogic.Auth;
using ReMi.Common.Utils;
using ReMi.Contracts.Cqrs.Events;
using ReMi.Contracts.Cqrs.Queries;
using ReMi.Queries.Auth;

namespace ReMi.Api.Insfrastructure.Notifications
{
    public class SubscriptionManager : ISubscriptionManager
    {
        private static readonly ILog Logger = LogManager.GetCurrentClassLogger();

        private static readonly ConcurrentDictionary<string, ClientRegistration> RegisteredClients = new ConcurrentDictionary<string, ClientRegistration>();

        public IAccountsBusinessLogic AccountsBusinessLogic { get; set; }
        public INotificationFilterApplying NotificationFilterApplying { get; set; }
        public ISerialization Serialization { get; set; }
        public IHandleQuery<GetAccountRequest, GetAccountResponse> GetAccountQuery { get; set; }
        
        public void Register(string connectionId, string token)
        {
            Logger.InfoFormat("Register ConnectionId={0}, Token={1}", connectionId, token);

            var parts = HttpTokenHelper.GetTokenParts(token);

            if (parts != null)
            {
                var registration = new ClientRegistration
                {
                    ConnectionId = connectionId,
                    Token = token,
                    SessionId = parts.SessionId,
                    UserName = parts.UserName,
                };

                if (RegisteredClients.TryAdd(token, registration))
                    Logger.DebugFormat("[ConnectionId={0}] Registered client {1}", registration.ConnectionId, registration);
                else
                    Logger.WarnFormat("[ConnectionId={0}] Failed to regsiter client {1}", registration.ConnectionId, registration);
            }
            else
            {
                Logger.WarnFormat("[ConnectionId={0}] Invalid token={1}", connectionId, token);
            }
        }

        public void Subscribe(string connectionId, string eventName, string filterJson)
        {
            Logger.DebugFormat("[connectionId={0}] Subscribe Event={1}, Filter={2}", 
                connectionId, eventName, filterJson);

            var registration = FindByConnectionId(connectionId);
            if (registration == null)
            {
                Logger.WarnFormat("[connectionId={0}] Registration not found. Event={1}, Filter={2}", 
                    connectionId, eventName, filterJson);
                return;
            }
            if (registration.Subscriptions == null)
            {
                Logger.WarnFormat("[connectionId={0}] Registration has no subscriptions. Event={1}, Filter={2}", 
                    connectionId, eventName, filterJson);
                return;
               
            }

            var frontendFilters = ParseFrontendFilter(filterJson);

            var subscription = registration.Subscriptions.Find(o => o.EventName == eventName);
            if (subscription != null)
            {
                foreach (var frontendFilter in frontendFilters)
                {
                    if (!subscription.Filters.Contains(frontendFilter))
                    {
                        subscription.Filters.Add(frontendFilter);

                        Logger.DebugFormat("[connectionId={0}] Subscription Event={1}, Filter={2} added to existing {3}", 
                            connectionId, eventName, filterJson, subscription);
                    }
                }
            }
            else
            {
                registration.Subscriptions.Add(new Subscription
                {
                    EventName = eventName,
                    Filters = frontendFilters
                });

                Logger.DebugFormat("[connectionId={0}] Subscription created for Event={1}, Filter={2}", 
                    connectionId, eventName, filterJson);
            }
        }

        public void Unsubscribe(string connectionId, string eventName, string filterJson)
        {
            Logger.DebugFormat("[ConnectionId={0}] Unsubscribe from Event={1}, Filter={2}", 
                connectionId, eventName, filterJson);

            var frontendFilter = new List<SubscriptionFilter>();
            if (!string.IsNullOrWhiteSpace(filterJson))
                frontendFilter = ParseFrontendFilter(filterJson);

            var registration = FindByConnectionId(connectionId);
            if (registration != null)
            {
                if (registration.Subscriptions == null)
                {
                    Logger.WarnFormat("[connectionId={0}] Registration has no subscriptions. Event={1}, Filter={2}", 
                        connectionId, eventName, filterJson);
                }
                else
                {
                    if (!frontendFilter.Any())
                    {
                        registration.Subscriptions.RemoveAll(o => o.EventName == eventName);
                        Logger.DebugFormat("[ConnectionId={0}] Subscription removed Event={1}, Filter={2}", 
                            connectionId, eventName, filterJson);
                    }
                    else
                    {
                        registration.Subscriptions.RemoveAll(
                            o => o.EventName == eventName && frontendFilter.SequenceEqual(o.Filters));

                        Logger.WarnFormat("[ConnectionId={0}] Filtered subscription removed Event={1}, Filter={2}",
                            connectionId, eventName, filterJson);
                    }
                }
            }

            Logger.DebugFormat("[ConnectionId={0}] Current subscriptions after removal. Registration={1}", 
                connectionId, registration);
        }

        public void Unsubscribe(string connectionId, string eventName)
        {
            Unsubscribe(connectionId, eventName, null);
        }

        public void UnRegisterByToken(string token)
        {
            Logger.DebugFormat("[token={0}] UnRegisterByToken", token);

            var prevRegistration = FindByToken(token);
            if (prevRegistration != null)
            {
                RegisteredClients.TryRemove(token, out prevRegistration);

                Logger.DebugFormat("[ConnectionId={0}] Unregistered connection {1}", 
                    prevRegistration.ConnectionId, prevRegistration);
            }
        }

        public void UnRegisterByConnectionId(string connectionId)
        {
            Logger.DebugFormat("[ConnectionId={0}] UnRegisterByConnectionId", connectionId);

            var registration = FindByConnectionId(connectionId);
            if (registration != null)
                UnRegisterByToken(registration.Token);

            Logger.DebugFormat("[ConnectionId={0}] Unregistered connection {1}", connectionId, registration);
        }

        public bool IsRegistered(string token)
        {
            var registration = FindByToken(token);
            return (registration != null);
        }

        public bool IsRegisteredConnectionId(string connectionId)
        {
            var registration = FindByConnectionId(connectionId);
            return (registration != null);
        }

        public string[] FilterSubscribers(IEvent evnt)
        {
            var registrations = RegisteredClients.ToArray();

            Logger.DebugFormat("Event {0} has subscriptions {1}", evnt.GetType().Name, registrations);

            return
                registrations
                    .Select(o => new { o.Value.ConnectionId, Account = GetAccountFromRegistration(o.Value.ConnectionId), o.Value.Subscriptions })
                    .Where(o => NotificationFilterApplying.Apply(evnt, o.Account, o.Subscriptions))
                    .Select(o => o.ConnectionId)
                    .ToArray();
        }

        #region Helpers

        private Account GetAccountFromRegistration(string connectionId)
        {
            var registration = FindByConnectionId(connectionId);
            if (registration == null)
                throw new Exception(string.Format("Request connection not registered. ConnectionId={0}", connectionId));

            var session = AccountsBusinessLogic.GetSession(registration.SessionId);
            if (session == null)
                throw new Exception(string.Format("Session not found. ConnectionId={0}, SessionId={1}", connectionId, registration.SessionId));

            var account = GetAccountQuery.Handle(new GetAccountRequest {AccountId = session.AccountId}).Account;
            if (account == null)
                throw new Exception(string.Format("Account not found. ConnectionId={0}, AccountId={1}", connectionId, session.ExternalId));

            return account;
        }

        private ClientRegistration FindByToken(string token)
        {
            ClientRegistration found;
            if (RegisteredClients.TryGetValue(token, out found))
                return found;

            return null;
        }

        private ClientRegistration FindByConnectionId(string connectionId)
        {
            var found = RegisteredClients.FirstOrDefault(o => o.Value.ConnectionId == connectionId);
            if (found.Value != null)
                return found.Value;

            return null;
        }

        private SubscriptionFilters ParseFrontendFilter(string filterJson)
        {
            var result = new SubscriptionFilters();

            var frontendFilters = Serialization.FromJson<FrontendFilter>(filterJson);
            if (frontendFilters != null)
            {
                result.AddRange(frontendFilters.Select(filter => new SubscriptionFilter
                {
                    Property = filter.Key,
                    Value = filter.Value
                }));
            }

            return result;
        }

        #endregion
    }

}
