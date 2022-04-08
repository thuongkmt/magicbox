using Konbini.RfidFridge.Domain.DTO;
using Konbini.RfidFridge.Service.Core;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Collections.Generic;
using System.Text;

namespace Konbini.RfidFridge.Service.Util
{
    using Konbini.RfidFridge.Domain.DTO.DeviceChecking;
    using Konbini.RfidFridge.Domain.Enums;

    public class RabbitMqService
    {
        private LogService LogService;
        public RabbitMqService(LogService logService)
        {
            LogService = logService;
        }

        public bool Init()
        { 
            try
            {
                var factory = new ConnectionFactory() { HostName = "localhost" };
                var connection = factory.CreateConnection();
                return true;
            }
            catch (System.Exception ex)
            {
               // LogService.LogError(ex);
                return false;
            }
        }
        public void PublishTagId(List<StateTagIdDto> tags)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: "tagid", type: "fanout");
                var json = JsonConvert.SerializeObject(tags);

                var body = Encoding.UTF8.GetBytes(json);
                channel.BasicPublish(exchange: "tagid", routingKey: "", basicProperties: null, body: body);
            }
        }

        //public void PublishStateTagId(List<TagIdDto> tags)
        //{
        //    var factory = new ConnectionFactory() { HostName = "localhost" };
        //    using (var connection = factory.CreateConnection())
        //    using (var channel = connection.CreateModel())
        //    {
        //        channel.ExchangeDeclare(exchange: "tagid", type: "fanout");
        //        var json = JsonConvert.SerializeObject(tags);

        //        var body = Encoding.UTF8.GetBytes(json);
        //        channel.BasicPublish(exchange: "tagid", routingKey: "", basicProperties: null, body: body);
        //    }
        //}


        //public void PublishMachineStatus(MachineStatus status)
        //{
        //    var factory = new ConnectionFactory() { HostName = "localhost" };
        //    using (var connection = factory.CreateConnection())
        //    using (var channel = connection.CreateModel())
        //    {
        //        channel.ExchangeDeclare(exchange: "machinestatus", type: "fanout");
        //        //var json = JsonConvert.SerializeObject(products);

        //        var body = Encoding.UTF8.GetBytes(status.ToString());
        //        channel.BasicPublish(exchange: "machinestatus", routingKey: "", basicProperties: null, body: body);
        //    }
        //}

        public void PublishMachineStatus(MachineStatus status, string message = null)
        {

            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: "machinestatus", type: "fanout");
                var dataToSend = new MachineStatusMessageDto
                {
                    Status = status.ToString(),
                    Message = message
                };
                string json = JsonConvert.SerializeObject(dataToSend);
                var body = Encoding.UTF8.GetBytes(json.ToString());
                channel.BasicPublish(exchange: "machinestatus", routingKey: "", basicProperties: null, body: body);
            }
        }

        public void PublishOrder(OrderDto orderDto)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: "order", type: "fanout");
                var json = JsonConvert.SerializeObject(orderDto);

                var body = Encoding.UTF8.GetBytes(json);
                channel.BasicPublish(exchange: "order", routingKey: "", basicProperties: null, body: body);
            }
            // Console.WriteLine("PUB ORDER");
        }

        public void PublishInventory(List<InventoryDto> inventories)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: "inventory", type: "fanout");
                var json = JsonConvert.SerializeObject(inventories);

                var body = Encoding.UTF8.GetBytes(json);
                channel.BasicPublish(exchange: "inventory", routingKey: "", basicProperties: null, body: body);
            }
            // Console.WriteLine("PUB INVEN");
        }

        public void PublishMissedInventory(List<InventoryDto> inventories)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: "missed-inventory", type: "fanout");
                var json = JsonConvert.SerializeObject(inventories);

                var body = Encoding.UTF8.GetBytes(json);
                channel.BasicPublish(exchange: "missed-inventory", routingKey: "", basicProperties: null, body: body);
            }
            // Console.WriteLine("PUB INVEN");
        }


        public void PublishTemperature(TemperatureDto temperatures)
        {
            var topic = "temperatures";

            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: topic, type: "fanout");

                var json = JsonConvert.SerializeObject(temperatures);

                var body = Encoding.UTF8.GetBytes(json);
                channel.BasicPublish(exchange: topic, routingKey: "", basicProperties: null, body: body);
            }
            // Console.WriteLine("PUB INVEN");
        }

        public void PublishUIMessage(string message)
        {
            var topic = "messages";

            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: topic, type: "fanout");

                var json = JsonConvert.SerializeObject(message);

                var body = Encoding.UTF8.GetBytes(json);
                channel.BasicPublish(exchange: topic, routingKey: "", basicProperties: null, body: body);
            }
            // Console.WriteLine("PUB INVEN");
        }

        public void PublishUIDialogMessage(DialogMessageDTO message)
        {
            var topic = "dialog-messages";

            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: topic, type: "fanout");

                var json = JsonConvert.SerializeObject(message);

                var body = Encoding.UTF8.GetBytes(json);
                channel.BasicPublish(exchange: topic, routingKey: "", basicProperties: null, body: body);
            }
            // Console.WriteLine("PUB INVEN");
        }



        public void PublishDeviceCheckingList(List<DeviceCheckingDTO> message)
        {
            var topic = "device-checking";

            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: topic, type: "fanout");

                var json = JsonConvert.SerializeObject(message);

                var body = Encoding.UTF8.GetBytes(json);
                channel.BasicPublish(exchange: topic, routingKey: "", basicProperties: null, body: body);
            }
            // Console.WriteLine("PUB INVEN");
        }



    }
}
