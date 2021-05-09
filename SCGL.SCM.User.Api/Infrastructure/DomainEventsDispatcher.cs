﻿using Autofac;
using Autofac.Core;
using Lazarus.Common.Application;
using Lazarus.Common.Domain.Seedwork;
using Lazarus.Common.EventMessaging;
using Lazarus.Common.infrastructure;
using MediatR;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SCGL.SCM.User.Api.Infrastructure
{
    public class DomainEventsDispatcher : IDomainEventsDispatcher
    {
        private readonly IMediator _mediator;
        private readonly ILifetimeScope _scope;
        private readonly DbDataContext _db;
        private readonly IEventBus _eventBus;

        public DomainEventsDispatcher(IMediator mediator, ILifetimeScope scope, DbDataContext db, IEventBus eventBus)
        {
            _eventBus = eventBus;
            this._mediator = mediator;
            this._scope = scope;
            this._db = db;
        }

        public async Task DispatchEventsAsync()
        {
            var domainEntities = this._db.ChangeTracker
                .Entries<Entity>()
                .Where(x => x.Entity.DomainEvents != null && x.Entity.DomainEvents.Any()).ToList();

            var domainEvents = domainEntities
                .SelectMany(x => x.Entity.DomainEvents)
                .ToList();

            var domainEventNotifications = new List<IDomainEventNotification<IDomainEvent>>();
            foreach (var domainEvent in domainEvents)
            {
                Type domainEvenNotificationType = typeof(IDomainEventNotification<>);
                var domainNotificationWithGenericType = domainEvenNotificationType.MakeGenericType(domainEvent.GetType());
                var domainNotification = _scope.ResolveOptional(domainNotificationWithGenericType, new List<Parameter>
                {
                    new NamedParameter("domainEvent", domainEvent)
                });

                if (domainNotification != null)
                {
                    domainEventNotifications.Add(domainNotification as IDomainEventNotification<IDomainEvent>);
                }
            }

            domainEntities
                .ForEach(entity => entity.Entity.ClearDomainEvents());

            //var tasks = domainEvents
            //    .Select(async (domainEvent) =>
            //    {
            //        await _mediator.Publish(domainEvent);
            //    });

            foreach (var item in domainEvents)
            {
                var @event = (IntegrationEvent)item;
                _eventBus.Publish(@event);
            }
            //await Task.WhenAll(tasks);

            foreach (var domainEventNotification in domainEventNotifications)
            {
                string type = domainEventNotification.GetType().FullName;
                var data = JsonConvert.SerializeObject(domainEventNotification);
                //OutboxMessage outboxMessage = new OutboxMessage(
                //    domainEventNotification.DomainEvent.OccurredOn,
                //    type,
                //    data);
                //this._ordersContext.OutboxMessages.Add(outboxMessage);
            }
        }
    }
}
