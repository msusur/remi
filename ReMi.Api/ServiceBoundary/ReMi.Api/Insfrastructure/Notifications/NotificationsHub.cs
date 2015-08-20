using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Common.Logging;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using ReMi.Common.Utils;
using ReMi.Common.WebApi.Notifications;
using ReMi.Contracts.Cqrs.Events;

namespace ReMi.Api.Insfrastructure.Notifications
{
    [HubName("notificationsHub")]
    public class NotificationsHub : Hub, IFrontendNotificator
    {
        private static readonly ILog Logger = LogManager.GetCurrentClassLogger();

        private readonly ISerialization _serialization;
        private readonly ISubscriptionManager _subscriptionManager;

        public NotificationsHub()
        {
            _serialization = Resolve<ISerialization>();
            _subscriptionManager = Resolve<ISubscriptionManager>();
        }

        public NotificationsHub(ISerialization serialization, ISubscriptionManager subscriptionManager)
        {
            _serialization = serialization;
            _subscriptionManager = subscriptionManager;
        }

        private T Resolve<T>()
        {
            return (T)GlobalConfiguration.Configuration.DependencyResolver.GetService(typeof(T));
        }


        #region Client-to-Server

        [HubMethodName("register")]
        public void Register(string token)
        {
            if (_subscriptionManager.IsRegistered(token))
                UnRegister(token);

            _subscriptionManager.Register(Context.ConnectionId, token);
        }

        [HubMethodName("unregister")]
        public void UnRegister(string token)
        {
            if (_subscriptionManager.IsRegistered(token))
            {
                _subscriptionManager.UnRegisterByToken(token);
            }
        }

        [HubMethodName("subscribe")]
        public void Subscribe(string eventName, string filter)
        {
            _subscriptionManager.Subscribe(Context.ConnectionId, eventName, filter);
        }

        [HubMethodName("unsubscribe")]
        public void Unsubscribe(string eventName, string filter)
        {
            _subscriptionManager.Unsubscribe(Context.ConnectionId, eventName, filter);
        }

        [HubMethodName("unsubscribeall")]
        public void Unsubscribe(string eventName)
        {
            _subscriptionManager.Unsubscribe(Context.ConnectionId, eventName);
        }

        #endregion


        #region Server-to-Client

        public void NotifyFiltered<T>(T evnt) where T : IEvent
        {
            Logger.DebugFormat("Notify filtered Event={0}", evnt.GetType().Name);

            var json = _serialization.ToJson(evnt);

            var hubContext = GlobalHost.ConnectionManager.GetHubContext<NotificationsHub>();

            var clients = _subscriptionManager.FilterSubscribers(evnt);
            
            if (clients.Any())
                foreach (var client in clients)
                {
                    hubContext.Clients.Client(client).notify(typeof(T).Name, json);

                    Logger.DebugFormat("Notification Event={0} sent to Client={1}", typeof(T).Name, client);
                }
        }

        public void NotifyGlobal<T>(T evnt) where T : IEvent
        {
            var json = _serialization.ToJson(evnt);

            Logger.DebugFormat("Notify all clients about Event={0}", typeof(T).Name);

            var hubContext = GlobalHost.ConnectionManager.GetHubContext<NotificationsHub>();
            hubContext.Clients.All.notify(typeof(T).Name, json);
        }

        #endregion


        #region Hub events

        public override Task OnConnected()
        {
            Logger.InfoFormat("[ConnectionId={0}] Client connected", Context.ConnectionId);

            return base.OnConnected();
        }

        public override Task OnDisconnected()
        {
            _subscriptionManager.UnRegisterByConnectionId(Context.ConnectionId);

            Logger.InfoFormat("[ConnectionId={0}] Client disconnected", Context.ConnectionId);

            return base.OnDisconnected();
        }

        public override Task OnReconnected()
        {
            Logger.InfoFormat("[ConnectionId={0}] Client reconnected", Context.ConnectionId);

            return base.OnReconnected();
        }

        #endregion


        #region Helpers


        #endregion
    }

}
