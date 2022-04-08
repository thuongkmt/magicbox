//using System;
//using System.Text;
//using System.Threading.Tasks;
//using Abp.Dependency;
//using Castle.Core.Logging;
//using KonbiCloud.Configuration;
//using KonbiCloud.Messaging.Events;
//using Konbini.KonbiCloud.Azure;
//using Microsoft.Azure.ServiceBus;
//using Microsoft.Azure.ServiceBus.Management;
//using Microsoft.Extensions.Configuration;
//using Newtonsoft.Json;

//namespace KonbiCloud.Azure
//{
//    public  class ServiceBusService: KonbiCloudAppServiceBase, IServiceBusService ,ISingletonDependency
//    {
//        private readonly ManagementClient _managementClient;
//        private const string INTEGRATION_EVENT_SUFIX = "IntegrationEvent";

//        private readonly ILogger _logger;
//        private readonly EventBusConfiguration eventBusConfiguration;

//        private string currentTenantName;
         
//        public ServiceBusService(IConfiguration _appConfiguration,ILogger logger)
//        {
//            _logger = logger;
//            eventBusConfiguration = new EventBusConfiguration();
//            _appConfiguration.GetSection("EventBus").Bind(eventBusConfiguration);
//            _managementClient = new ManagementClient(eventBusConfiguration.ConnectionString);
//        }

//        public async Task Publish(IntegrationEvent @event, string topic)
//        {
//            var eventName = @event.GetType().Name.Replace(INTEGRATION_EVENT_SUFIX, "");
//            var jsonMessage = JsonConvert.SerializeObject(@event);
//            var body = Encoding.UTF8.GetBytes(jsonMessage);
//            _logger.Info(string.Format("{0}:{1}", eventName, JsonConvert.SerializeObject(@event)));

//            var message = new Message
//            {
//                MessageId = new Guid().ToString(),
//                Body = body,
//                Label = eventName
//            };
//            var topicName = $"{await GetTenantName()}_{topic}";
//            var topicClient= new TopicClient(eventBusConfiguration.ConnectionString, topicName);
//            await topicClient.SendAsync(message);
//            await topicClient.CloseAsync();
//        }

//        public async Task PublishCurrentTenant(IntegrationEvent @event)
//        {
//            var eventName = @event.GetType().Name.Replace(INTEGRATION_EVENT_SUFIX, "");
//            var jsonMessage = JsonConvert.SerializeObject(@event);
//            var body = Encoding.UTF8.GetBytes(jsonMessage);
//            _logger.Info(string.Format("{0}:{1}", eventName, JsonConvert.SerializeObject(@event)));

//            var message = new Message
//            {
//                MessageId = new Guid().ToString(),
//                Body = body,
//                Label = eventName
//            };


//            var topicClient = new TopicClient(eventBusConfiguration.ConnectionString, await GetTenantName());
//            await topicClient.SendAsync(message);
//            await topicClient.CloseAsync();
//        }

//        private async Task<string> GetTenantName()
//        {
//            if (string.IsNullOrEmpty(currentTenantName))
//            {
//                var tenant = await GetCurrentTenantAsync();
//                currentTenantName = tenant.TenancyName;
//            }
//            return currentTenantName;
//        }

//        public async Task CreateSubscription(string machineId)
//        {
//            try
//            {
//                var topicName = await GetTenantName();
//                if (!await _managementClient.TopicExistsAsync(topicName))
//                    await _managementClient.CreateTopicAsync(new TopicDescription(topicName));
//                if (!await _managementClient.SubscriptionExistsAsync(topicName, machineId))
//                    await _managementClient.CreateSubscriptionAsync(topicName, machineId);
//            }
//            catch (Exception e)
//            {
//               Logger.Error("CreateSubscription",e);
//            }
           
//        }
//    }
//}
