using Autofac;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Lazarus.Common.DI;
using Lazarus.Common.EventMessaging.EventStore;
using Lazarus.Common.Utilities;
using System.Collections.Generic;
using System.Linq;
using Lazarus.Common.CQRS.Command;
using Lazarus.Common.CQRS.Implement;
using Lazarus.Common.Interface;
using System.Threading;
using Lazarus.Common.ExceptionHandling;
using Confluent.Kafka;
using Lazarus.Common.Nexus.Database;

namespace Lazarus.Common.EventMessaging.Implement
{
    public class EventBusKafka : IEventBus, IDisposable
    {
        public IEventStore _EventStore;
        private readonly IEventBusSubscriptionsManager _subsManager;
        private readonly ILifetimeScope _autofac;
        private readonly int _retryCount;
        
        ICommandHandlerExecutor CommandHandlerExecutor { get; }

        public EventBusKafka(
            ICommandHandlerExecutor commandHandlerExecutor,
            IEventBusSubscriptionsManager subsManager, IEventStore eventStore)
        {
            CommandHandlerExecutor = commandHandlerExecutor;
            _EventStore = eventStore;
            _subsManager = subsManager ?? new InMemoryEventBusSubscriptionsManager();
            _retryCount = 5;
            _subsManager.OnEventRemoved += SubsManager_OnEventRemoved;



        }

        public async Task Consume<TCommand>(TCommand command,string consumerGroup,string topic) where TCommand : IntegrationEvent
        {
            throw new NotImplementedException();
            //try
            //{
            //    await ProccessBus(command);
            //  var log=  _EventStore.ConsumeSuccess(command.AggregateId, consumerGroup, command.ToJSON(), topic);

            //    Publish(log, "LogEventConsumer");
            //}
            //catch (Exception e)
            //{
            //    var log = _EventStore.ConsumeFail(command.AggregateId, consumerGroup, command.ToJSON(), topic,e.GetMessageError());
            //    Publish(log, "LogEventConsumer");
            //    throw e;
            //}
       
        }
        private void SubsManager_OnEventRemoved(object sender, string eventName)
        {

        }
        async Task ProccessBus(ICommand command)
        {
            var commandType = command.GetType();
            var executorType = CommandHandlerExecutor.GetType();

            await (Task)executorType.GetMethod(nameof(ICommandHandlerExecutor.Execute))
                     .MakeGenericMethod(commandType)
                     .Invoke(CommandHandlerExecutor, new[] { command });
        }

        public void Publish(IntegrationEvent @event)
        {
        
            var env = AppConfigUtilities.GetAppConfig<string>("KAFKA_ENV");
  

            var eventName = @event.GetType().Name;
            var kafkaUrl = AppConfigUtilities.GetAppConfig<string>("KAFKA_URL");
            var config = new ProducerConfig { BootstrapServers = kafkaUrl };
            var topic = env + eventName;
            if (@event.AggregateId.IsNullOrEmpty())
                @event.AggregateId = Guid.NewGuid().ToString();
            var msg = new Message<string, string> { Key = @event.AggregateId, Value = @event.ToJSON() };
            using (var p = new ProducerBuilder<string, string>(config).Build())
            {
            
                var result= p.ProduceAsync(topic, msg).Result;

                _EventStore.Persist(@event, result.Offset.Value);
            }


        }

        public void PublishAll(List<IntegrationEvent> @events)
        {
            if (@events.Count == 0) return;
            var env = AppConfigUtilities.GetAppConfig<string>("KAFKA_ENV");

            var publisher = Environment.MachineName;
            var eventName = @events.FirstOrDefault().GetType().Name;
            var kafkaUrl = AppConfigUtilities.GetAppConfig<string>("KAFKA_URL");
            var config = new ProducerConfig { BootstrapServers = kafkaUrl };
            var topic = env + eventName;
            var p = new ProducerBuilder<string, string>(config).Build();
            List<(IntegrationEvent, long)> logs = new List<(IntegrationEvent, long)>();
            foreach (var item in @events)
            {
                if (item.AggregateId.IsNullOrEmpty())
                    item.AggregateId = Guid.NewGuid().ToString();
                var msg = new Message<string, string> { Key = item.AggregateId, Value = item.ToJSON() };
            

                    var result = p.ProduceAsync(topic, msg).Result;

                #region ----LOG ES----
                var l = new LogEventStore();
                l.AggregateId = item.AggregateId;
                l.CreateDate = DateTime.Now.GetLocalDate();
                l.EventName = env + item.GetType().Name;
                l.Val = item.ToJSON();
                l.OffSet = result.Offset;
                l.Publisher = publisher;
                var msgLog = new Message<string, string> { Key = l.AggregateId, Value = l.ToJSON() };
                var log = p.ProduceAsync("LogEventStore", msgLog).Result;

                #endregion

                logs.Add((item, result.Offset.Value));
            }
            _EventStore.PersistAll(logs);

        }

        public void Publish(object obj,string _topic)
        {
            var env = AppConfigUtilities.GetAppConfig<string>("KAFKA_ENV");
         
            var kafkaUrl = AppConfigUtilities.GetAppConfig<string>("KAFKA_URL");
            var config = new ProducerConfig { BootstrapServers = kafkaUrl };

            var msg = new Message<string, string> { Key = Guid.NewGuid().ToString(), Value = obj.ToJSON() };
            using (var p = new ProducerBuilder<string, string>(config).Build())
            {

               var result= p.ProduceAsync(_topic, msg).Result;
 
            }


        }

        public void SubscribeDynamic<TH>(string eventName)
            where TH : IDynamicIntegrationEventHandler
        {
            //_logger.LogInformation("Subscribing to dynamic event {EventName} with {EventHandler}", eventName, typeof(TH).GetGenericTypeName());


            _subsManager.AddDynamicSubscription<TH>(eventName);

        }

        public void Subscribe<T, TH>()
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>
        {
            var eventName = _subsManager.GetEventKey<T>();

            _subsManager.AddSubscription<T, TH>();


        }



        public void Unsubscribe<T, TH>()
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>
        {
            var eventName = _subsManager.GetEventKey<T>();

            //_logger.LogInformation("Unsubscribing from event {EventName}", eventName);

            _subsManager.RemoveSubscription<T, TH>();
        }

        public void UnsubscribeDynamic<TH>(string eventName)
            where TH : IDynamicIntegrationEventHandler
        {
            _subsManager.RemoveDynamicSubscription<TH>(eventName);
        }

        public void Dispose()
        {

            _subsManager.Clear();
        }

        public void StartBasicConsume()
        {
            //    var env = AppConfigUtilities.GetAppConfig<string>("KAFKA_ENV");
            //    var consumer = DomainEvents._Consumer;


            //    consumer.OnPartitionsAssigned += (obj, partitions) =>
            //    {

            //        var fromBeginning = partitions.Select(p => new RdKafka.TopicPartitionOffset(p.Topic, p.Partition, RdKafka.Offset.Stored)).ToList();
            //        consumer.Assign(fromBeginning);
            //    };
            //    consumer.OnMessage += Consumer_OnMessage;
            //    var events = this._subsManager.Events.Select(s => env + s).ToList();

            //    consumer.Subscribe(events);
            //    consumer.Start();
            //}

            //private void Consumer_OnMessage(object sender, Message e)
            //{
            //    var isCommitSuccess = false;
            //    var i = 1;
            //    while (!isCommitSuccess)
            //    {
            //        if (i > 10)
            //        {
            //            isCommitSuccess = true;
            //            return;
            //        }
            //        var env = AppConfigUtilities.GetAppConfig<string>("KAFKA_ENV");
            //        var log = DomainEvents._Container.Resolve<ILogRepository>();
            //        string text = Encoding.UTF8.GetString(e.Payload, 0, e.Payload.Length);
            //        var eStore = DomainEvents._Container.Resolve<IEventStore>();
            //        var id = Encoding.UTF8.GetString(e.Key, 0, e.Key.Length);
            //        var gKafka = AppConfigUtilities.GetAppConfig<string>("KAFKA_CONSUMER_GROUP_ID");
            //        var envName = Environment.MachineName;

            //        try
            //        {

            //            var topic = e.Topic.Replace(env, "");
            //            ProcessEvent(topic, text).Wait();

            //            DomainEvents._Consumer.Commit(e).Wait();


            //            eStore.Commit(id, envName);
            //            isCommitSuccess = true;
            //        }
            //        catch(BusinessException busnessEx)
            //        {
            //            eStore.CommitedFail(id,     busnessEx.GetMessageError());
            //            isCommitSuccess = true;
            //        }
            //        catch (Exception exception)
            //        {

            //            eStore.CommitedFail(id, $"({i})" + exception.GetMessageError());
            //            log.Error($"({i})[{e.Topic}]" + exception.GetMessageError(), exception.StackTrace, "Consumer_OnMessage", null);
            //            isCommitSuccess = false;
            //            i++;
            //            Thread.Sleep(5000);
            //        }

            //    }
            throw new NotImplementedException();

        }



        private async Task ProcessEvent(string eventName, string message)
        {
            //_logger.LogTrace("Processing RabbitMQ event: {EventName}", eventName);

            if (_subsManager.HasSubscriptionsForEvent(eventName))
            {
                using (var scope = DomainEvents._Container.BeginLifetimeScope())
                {
                    var subscriptions = _subsManager.GetHandlersForEvent(eventName);
                    foreach (var subscription in subscriptions)
                    {

                        var handler = scope.ResolveOptional(subscription.HandlerType);
                        if (handler == null) continue;
                        var eventType = _subsManager.GetEventTypeByName(eventName);
                        var integrationEvent = JsonConvert.DeserializeObject(message, eventType);
                        var concreteType = typeof(IIntegrationEventHandler<>).MakeGenericType(eventType);

                        await Task.Yield();
                        await (Task)concreteType.GetMethod("Handle").Invoke(handler, new object[] { integrationEvent });

                    }
                }
            }
            else
            {
                //_logger.LogWarning("No subscription for RabbitMQ event: {EventName}", eventName);
            }
        }
    }
}