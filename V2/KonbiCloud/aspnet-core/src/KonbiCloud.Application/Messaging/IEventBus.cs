using System.Threading.Tasks;
using KonbiCloud.Messaging.Events;

namespace KonbiCloud.Messaging
{
    public interface IEventBus
    {
        void Publish(IntegrationEvent @event);
        Task Publish(IntegrationEvent @event, string label);
        Task CreateSubscription(string machineId);
    }
}
