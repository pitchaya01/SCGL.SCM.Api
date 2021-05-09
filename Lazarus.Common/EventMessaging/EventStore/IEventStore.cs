using Lazarus.Common.Nexus.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lazarus.Common.EventMessaging.EventStore
{
    public interface IEventStore
    {
        void Persist<TAggregate>(TAggregate aggregate,long offset) where TAggregate : IntegrationEvent;
        void PersistAll<TAggregate>(List<(TAggregate, long)> aggregates) where TAggregate : IntegrationEvent;
        bool IsCommited(string consumerGroup,string topic,int offset);
        LogEventConsumer ConsumeSuccess(string logEventStoreId, string consumerGroupName,string val, string eventName, int? offset = null);
        LogEventConsumer ConsumeFail(string logEventStoreId, string consumerGroupName, string val, string eventName, string messageEror, int? offset = null);
        void CommitedFail(string id, string msg);
        //TAggregate GetById<TAggregate, TEvent>(Guid id) where TAggregate : IAggregateRoot, new() where TEvent : class, IEvent;

    }
}
