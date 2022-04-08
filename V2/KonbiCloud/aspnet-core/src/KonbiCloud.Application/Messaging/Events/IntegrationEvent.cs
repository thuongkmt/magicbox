using System;

namespace KonbiCloud.Messaging.Events
{
    public class IntegrationEvent
    {
        public IntegrationEvent()
        {
            EventId = Guid.NewGuid();
            EventCreationDate = DateTime.UtcNow;
        }
        public IntegrationEvent( DateTime createTime)
        {
            EventId = Guid.NewGuid();
            EventCreationDate = createTime;
        }

        public Guid EventId { get; }
        public DateTime EventCreationDate { get; }
    }
}
