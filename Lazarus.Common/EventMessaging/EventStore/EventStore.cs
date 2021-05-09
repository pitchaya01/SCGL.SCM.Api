using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Confluent.Kafka;
using Lazarus.Common.Nexus.Database;
using Lazarus.Common.Utilities;

namespace Lazarus.Common.EventMessaging.EventStore
{
    public class EventStore : IEventStore
    {
        public NexusDataContext _db;

        public EventStore(NexusDataContext db)
        {
            _db = db;
        }

        public void Commit(string id,string MachinesName)
        {
            throw new NotImplementedException();
        }

        public void CommitedFail(string id, string msg)
        {
            throw new NotImplementedException();

        }

        public LogEventConsumer ConsumeSuccess(string logEventStoreId, string consumerGroupName, string val,string eventName,int? offset=null)
        {
            var log = _db.LogEventStores.FirstOrDefault(s => s.AggregateId == logEventStoreId);
       

            var logConsumer = new LogEventConsumer();
            logConsumer.ConsumerGroupName = consumerGroupName;
            logConsumer.CreateDate = DateTime.Now.GetLocalDate();
            logConsumer.Status = "Success";
            logConsumer.EventName = eventName;
            logConsumer.OffSet = offset;
            if (log != null)
                logConsumer.LogEventStoreId = log.Id;
            logConsumer.Val = val;
            _db.LogEventConsumer.Add(logConsumer);
            _db.SaveChanges();
            Publish(logConsumer, "LogEventConsumer");
           // ElasticSearchUtilities.Insert(logConsumer.ToJSON(), "logeventconsumer");
            return logConsumer;
        }
        public LogEventConsumer ConsumeFail(string logEventStoreId, string consumerGroupName, string val, string eventName,string messageEror, int? offset = null)
        {
            var log = _db.LogEventStores.FirstOrDefault(s => s.AggregateId == logEventStoreId);


            var logConsumer = new LogEventConsumer();
            logConsumer.ConsumerGroupName = consumerGroupName;
            logConsumer.CreateDate = DateTime.Now.GetLocalDate();
            logConsumer.Status = "Fail";
            logConsumer.EventName = eventName;
            logConsumer.MessageError = messageEror;
            logConsumer.OffSet = offset;
            if (log != null)
                logConsumer.LogEventStoreId = log.Id;
            logConsumer.Val = val;

            _db.LogEventConsumer.Add(logConsumer);
            _db.SaveChanges();
            Publish(logConsumer, "LogEventConsumer");
            // ElasticSearchUtilities.Insert(logConsumer.ToJSON(), "logeventconsumer");
            return logConsumer;
        }

        public bool IsCommited(string consumerGroup,string topic,int offset)
        {
            return _db.LogEventConsumer.Any(a => a.ConsumerGroupName == consumerGroup && a.OffSet == offset&&a.EventName== topic);
        }

        public void Persist<TAggregate>(TAggregate aggregate, long offset) where TAggregate : IntegrationEvent
        {
            var l = new LogEventStore();
            using (var scope = new TransactionScope(TransactionScopeOption.Suppress))
            {
                var publisher = Environment.MachineName;
                var env = AppConfigUtilities.GetAppConfig<string>("KAFKA_ENV");
                var date = DateTime.Now.GetLocalDate();
                var val = aggregate.ToJSON();
            
                l.AggregateId = aggregate.AggregateId;
                l.CreateDate = date;
                l.OffSet = offset;
                l.Publisher = publisher;
                l.EventName = env + aggregate.GetType().Name;
                l.Val = val;
                _db.LogEventStores.Add(l);
                _db.SaveChanges();
            }
            Publish(l, "LogEventStore");
        }
        public void Publish(object obj, string _topic)
        {
            var env = AppConfigUtilities.GetAppConfig<string>("KAFKA_ENV");

            var kafkaUrl = AppConfigUtilities.GetAppConfig<string>("KAFKA_URL");
            var config = new ProducerConfig { BootstrapServers = kafkaUrl };

            var msg = new Message<string, string> { Key = Guid.NewGuid().ToString(), Value = obj.ToJSON() };
            using (var p = new ProducerBuilder<string, string>(config).Build())
            {

                var result = p.ProduceAsync(_topic, msg).Result;

            }


        }


        public void PersistAll<TAggregate>(List<(TAggregate, long)> aggregates) where TAggregate : IntegrationEvent
        {
            var publisher = Environment.MachineName;
            var env = AppConfigUtilities.GetAppConfig<string>("KAFKA_ENV");

            var date = DateTime.Now.GetLocalDate();
            using (var scope = new TransactionScope(TransactionScopeOption.Suppress))
            {
   
                foreach (var aggregate in aggregates)
                {
                    var val = aggregate.Item1.ToJSON();
                    var l = new LogEventStore();
                    l.AggregateId = aggregate.Item1.AggregateId;
                    l.CreateDate = date;
                    l.EventName = env + aggregate.Item1.GetType().Name;
                    l.Val = val;
                    l.OffSet = aggregate.Item2;
                    l.Publisher = publisher;
                    _db.LogEventStores.Add(l);
                }
                _db.SaveChanges();
            }
        }

 
    }
}
