using Lazarus.Common.CQRS.Command;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Lazarus.Common.EventMessaging
{
    public interface IEventBus
    {

        void Publish(IntegrationEvent @event);
        void PublishAll(List<IntegrationEvent> @events);
        void Publish(object obj, string _topic);
        void Subscribe<T, TH>()
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>;

        void SubscribeDynamic<TH>(string eventName)
            where TH : IDynamicIntegrationEventHandler;

        void UnsubscribeDynamic<TH>(string eventName)
            where TH : IDynamicIntegrationEventHandler;

        void Unsubscribe<T, TH>()
            where TH : IIntegrationEventHandler<T>
            where T : IntegrationEvent;

        void StartBasicConsume();
        Task Consume<TCommand>(TCommand command,string consumerGroup,string topic) where TCommand : IntegrationEvent;
    }
}