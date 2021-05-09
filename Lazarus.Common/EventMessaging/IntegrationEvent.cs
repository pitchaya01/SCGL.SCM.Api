using Lazarus.Common.CQRS.Command;
using Lazarus.Common.Domain.Seedwork;
using Lazarus.Common.Utilities;
using Newtonsoft.Json;
using System;

namespace Lazarus.Common.EventMessaging
{
    public class IntegrationEvent:ICommand, IDomainEvent
    {
        public IntegrationEvent()
        {
            AggregateId = Guid.NewGuid().ToString();
            CreationDate = DateTime.Now;
        }



        [JsonProperty]
        public string AggregateId { get;  set; } = Guid.NewGuid().ToString();

        [JsonProperty] public DateTime CreationDate { get; private set; } = DateTime.Now.GetLocalDate();

        public DateTime OccurredOn { get; set; }
    }
}