using Common.Logging;
using ReMi.Api.Insfrastructure.Security;
using ReMi.Common.Utils;
using ReMi.Common.WebApi.Exceptions;
using ReMi.Common.WebApi.Notifications;
using ReMi.Common.WebApi.Tracking;
using ReMi.Contracts.Cqrs.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Dependencies;

namespace ReMi.Api.Insfrastructure
{
    public class EventPublisher : IPublishEvent
    {
        public IFrontendNotificator FronendNotificator { get; set; }
        public ISerialization Serialization { get; set; }
        public IApplicationSettings ApplicationSettings { get; set; }

        private static readonly ILog Logger = LogManager.GetCurrentClassLogger();
        private readonly IDependencyResolver _resolver;

        public EventPublisher(IDependencyResolver resolver)
        {
            _resolver = resolver;
        }

        private T Resolve<T>()
        {
            return (T)_resolver.GetService(typeof(T));
        }

        private IEnumerable<T> ResolveAll<T>()
        {
            return _resolver.GetServices(typeof(T)).Cast<T>();
        }

        public Task[] Publish<TEvent>(TEvent evnt) where TEvent : IEvent
        {
            Logger.InfoFormat("Publishing event. EventType={0}", evnt.GetType().Name);

            return ResolveHandlersAndInvoke(evnt);
        }

        private Task[] ResolveHandlersAndInvoke<TEvent>(TEvent evnt) where TEvent : IEvent
        {
            var handlers = ResolveAll<IHandleEvent<TEvent>>().ToList();

            if (evnt.Context == null)
            {
                var principal = Thread.CurrentPrincipal as RequestPrincipal;
                if (principal != null)
                {
                    evnt.Context = new EventContext
                    {
                        UserEmail = principal.Account != null ? principal.Account.Email : string.Empty,
                        UserName = principal.Account != null ? principal.Account.FullName : string.Empty,
                        UserId = principal.Account != null ? principal.Account.ExternalId : Guid.Empty,
                        UserRole = principal.Account != null ? principal.Account.Role.Name : string.Empty,
                        Id = Guid.NewGuid()
                    };
                }
            }

            var tasks = new List<Task>();

            if (!handlers.Any())
                Logger.InfoFormat("No handler was found for the event. EventType={0}", evnt.GetType().Name);
            else
            {
                var tracker = Resolve<IEventTracker>();

                if (tracker == null)
                    Logger.Error(new TrackerNotImplementedException<IEventTracker>());
                else
                    tasks.Add(new TaskFactory(TaskScheduler.Current)
                        .StartNew(() => InvokeFoundHandlers(tracker, handlers, evnt)));
            }

            tasks.Add(new TaskFactory(TaskScheduler.Current)
                .StartNew(() => FronendNotificator.NotifyFiltered(evnt)));

            return tasks.ToArray();
        }

        private void InvokeFoundHandlers<TEvent>(IEventTracker tracker, IEnumerable<IHandleEvent<TEvent>> handlers, TEvent evnt) where TEvent : IEvent
        {
            foreach (var handler in handlers)
            {
                InvokeHandler(tracker, handler, evnt);
            }
        }

        private void InvokeHandler<TEvent>(IEventTracker tracker, IHandleEvent<TEvent> handler, TEvent evnt) where TEvent : IEvent
        {
            var eventId = Guid.NewGuid();

            try
            {
                if (handler == null)
                {
                    tracker.Finished(eventId, "Command not implemented");

                    var ex = new CommandHandlerNotImplementedException<TEvent>();
                    Logger.Error(ex);

                    throw ex;
                }

                tracker.CreateTracker(eventId, evnt.GetType().Name, evnt, handler.GetType().FullName);

                Logger.InfoFormat("Event '{0}' was handled: {1}", evnt.GetType().Name, FormatLogHelper.FormatEntry(Serialization, ApplicationSettings, evnt));

                tracker.Started(eventId);

                handler.Handle(evnt);

                tracker.Finished(eventId);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);

                if (ex is ApplicationException)
                    tracker.Finished(eventId, (ex as ApplicationException).Message);
                else
                    tracker.Finished(eventId, "Request failed!");
            }
        }
    }
}
