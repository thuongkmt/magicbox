using System.Threading.Tasks;
using KonbiCloud.Messaging.Events;

namespace Konbini.KonbiCloud.Azure
{
    public interface IServiceBusService
    {
        Task Publish(IntegrationEvent theEvent, string topic);
        Task PublishCurrentTenant(IntegrationEvent @event);
        Task CreateSubscription(string machineId);
    }
}
