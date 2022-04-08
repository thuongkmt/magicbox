using System;
using System.Collections.Generic;
using System.Text;

namespace KonbiCloud.Configuration
{
    public class AzureIoTHubOption
    {
        // Event Hub-compatible endpoint
        // az iot hub show --query properties.eventHubEndpoints.events.endpoint --name {your IoT Hub name}
        public string EventHubsCompatibleEndpoint { get; set; }
        // Event Hub-compatible name
        // az iot hub show --query properties.eventHubEndpoints.events.path --name {your IoT Hub name}
        public string EventHubsCompatiblePath { get; set; }
        // az iot hub policy show --name iothubowner --query primaryKey --hub-name {your IoT Hub name}
        public string IotHubSasKey { get; set; }
        public string IotHubSasKeyName { get; set; }
    }
}
