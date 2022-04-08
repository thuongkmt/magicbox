namespace KonbiCloud.Configuration
{
    public class EventBusConfiguration
    {
        public string ConnectionString { get; set; }
        public string AzureTopicName { get; set; }
        public string SubscriptionClientName { get; set; }
    }
}
