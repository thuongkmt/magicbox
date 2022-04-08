using Newtonsoft.Json.Serialization;
using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Newtonsoft.Json;
using Microsoft.Azure.ServiceBus.Management;
using Microsoft.Extensions.Logging;
using KonbiCloud.Messaging.Events;

namespace KonbiCloud.Messaging
{
    public class EventBusServiceBus : IEventBus
    {
        private readonly ILogger _logger;

        private const string INTEGRATION_EVENT_SUFIX = "IntegrationEvent";
        private static TopicClient _topicClient;
        private static ManagementClient _managementClient;
        private readonly string _topicName;
        public EventBusServiceBus(string cnn, string topicName,
            ILogger logger)
        {
            _logger = logger;
            _managementClient = new ManagementClient(cnn);
            _topicClient = new TopicClient(cnn, topicName);
            _topicName = topicName;
        }
        public void Publish(IntegrationEvent @event)
        {

            var eventName = @event.GetType().Name.Replace(INTEGRATION_EVENT_SUFIX, "");
            var jsonMessage = JsonConvert.SerializeObject(@event);
            var body = Encoding.UTF8.GetBytes(jsonMessage);
            _logger.LogInformation(string.Format("{0}:{1}", eventName, JsonConvert.SerializeObject(@event)));

            var message = new Message
            {
                MessageId = new Guid().ToString(),
                Body = body,
                Label = eventName
            };

            _topicClient.SendAsync(message)
                  .GetAwaiter()
                  .GetResult();
        }

        public async Task Publish(IntegrationEvent @event, string label)
        {
            _logger.LogInformation(string.Format("{0}:{1}", label, JsonConvert.SerializeObject(@event)));
            var jsonMessage = JsonConvert.SerializeObject(@event, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });
            var body = Encoding.UTF8.GetBytes(jsonMessage);
            var message = new Message
            {
                MessageId = Guid.NewGuid().ToString(),
                Body = body,
                Label = label
            };
            await _topicClient.SendAsync(message);
        }

        public async Task CreateSubscription(string machineId)
        {
            if (!await _managementClient.SubscriptionExistsAsync(_topicName, machineId))
                await _managementClient.CreateSubscriptionAsync(_topicName, machineId);
        }
    }
}
