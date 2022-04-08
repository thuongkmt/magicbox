
using Konbini.RfidFridge.Domain.DTO;
using Newtonsoft.Json;
using StompSharp;
using StompSharp.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Konbini.RfidFridge.Service.Util
{
    using Konbini.RfidFridge.Service.Core;
    using System.Threading.Tasks;
    using System.Timers;

    using Konbini.RfidFridge.Domain.Enums;

    public class StompService
    {
        private IStompClient client;

        private LogService LogService;

        public Action<string> OnCommandRev;

        public StompService(LogService logService)
        {
            LogService = logService;
        }
        public void Connect()
        {
            client = new StompClient("localhost", 61613);
        }

        public void PublishInventory(List<InventoryDto> inventories)
        {
            Task.Run(() =>
            {
                try
                {
                    var autoAck = client.SubscriptionBehaviors.AutoAcknowledge;
                    using (IDestination<IMessage> destination = client.GetDestination("/topic/inventory", autoAck))
                    {
                        var json = JsonConvert.SerializeObject(inventories);
                        var body = Encoding.UTF8.GetBytes(json);
                        var messageToSend = new BodyOutgoingMessage(body);
                        destination.SendAsync(messageToSend, NoReceiptBehavior.Default);
                    }
                }
                catch (Exception ex)
                {
                    LogService.LogError(ex);
                }
            });

        }

        public void PublishMissedInventory(List<InventoryDto> inventories)
        {
            Task.Run(() =>
            {
                try
                {
                    var autoAck = client.SubscriptionBehaviors.AutoAcknowledge;
                    using (IDestination<IMessage> destination = client.GetDestination("/topic/missed-inventory", autoAck))
                    {
                        var json = JsonConvert.SerializeObject(inventories);
                        var body = Encoding.UTF8.GetBytes(json);
                        var messageToSend = new BodyOutgoingMessage(body);
                        destination.SendAsync(messageToSend, NoReceiptBehavior.Default);
                    }
                }
                catch (Exception ex)
                {
                    LogService.LogError(ex);
                }
            });

        }

        public void PublishUnstableInventory(List<UnstableInventoryDto> inventories)
        {
            Task.Run(() =>
            {
                try
                {
                    var autoAck = client.SubscriptionBehaviors.AutoAcknowledge;
                    using (IDestination<IMessage> destination = client.GetDestination("/topic/unstable-inventory", autoAck))
                    {
                        var json = JsonConvert.SerializeObject(inventories);
                        var body = Encoding.UTF8.GetBytes(json);
                        var messageToSend = new BodyOutgoingMessage(body);
                        destination.SendAsync(messageToSend, NoReceiptBehavior.Default);
                    }
                }
                catch (Exception ex)
                {
                    LogService.LogError(ex);
                }
            });

        }

        public void PublishDashboard(DashboardDto dashboard)
        {
            Task.Run(() =>
            {
                try
                {
                    var autoAck = client.SubscriptionBehaviors.AutoAcknowledge;
                    using (IDestination<IMessage> destination = client.GetDestination("/topic/dashboard", autoAck))
                    {
                        var json = JsonConvert.SerializeObject(dashboard);
                        var body = Encoding.UTF8.GetBytes(json);
                        var messageToSend = new BodyOutgoingMessage(body);
                        destination.SendAsync(messageToSend, NoReceiptBehavior.Default);
                    }
                }
                catch (Exception ex)
                {
                    LogService.LogError(ex);
                }
            });
        }


        public void PublishProductSummarize(List<ProductSummarizeDto> data)
        {
            Task.Run(() =>
            {
                try
                {
                    var autoAck = client.SubscriptionBehaviors.AutoAcknowledge;
                    using (IDestination<IMessage> destination = client.GetDestination("/topic/productsummarize", autoAck))
                    {
                        var json = JsonConvert.SerializeObject(data);
                        var body = Encoding.UTF8.GetBytes(json);
                        var messageToSend = new BodyOutgoingMessage(body);
                        destination.SendAsync(messageToSend, NoReceiptBehavior.Default);
                    }
                }
                catch (Exception ex)
                {
                    LogService.LogError(ex);
                }
            });
        }


        public void PublishTemperature(TemperatureDto temperatures)
        {
            Task.Run(() =>
            {
                try
                {
                    var autoAck = client.SubscriptionBehaviors.AutoAcknowledge;
                    using (IDestination<IMessage> destination = client.GetDestination("/topic/temperatures", autoAck))
                    {
                        var json = JsonConvert.SerializeObject(temperatures);
                        var body = Encoding.UTF8.GetBytes(json);
                        var messageToSend = new BodyOutgoingMessage(body);
                        destination.SendAsync(messageToSend, NoReceiptBehavior.Default);
                    }
                }
                catch (Exception ex)
                {
                    LogService.LogError(ex);
                }
            });

        }

        //public void PublishTagId(List<StateTagIdDto> tags)
        //{
        //    Task.Run(() =>
        //    {
        //        try
        //        {
        //            var autoAck = client.SubscriptionBehaviors.AutoAcknowledge;
        //            using (IDestination<IMessage> destination = client.GetDestination("/topic/tagid", autoAck))
        //            {
        //                var json = JsonConvert.SerializeObject(tags);
        //                var body = Encoding.UTF8.GetBytes(json);
        //                var messageToSend = new BodyOutgoingMessage(body);
        //                destination.SendAsync(messageToSend, NoReceiptBehavior.Default);
        //            }

        //        }
        //        catch (Exception ex)
        //        {
        //            LogService.LogError(ex);
        //        }
        //    });

        //}

        public void PublishTagId(List<TagIdDto> tags)
        {
            Task.Run(() =>
            {
                try
                {
                    var autoAck = client.SubscriptionBehaviors.AutoAcknowledge;
                    using (IDestination<IMessage> destination = client.GetDestination("/topic/tagid", autoAck))
                    {
                        var json = JsonConvert.SerializeObject(tags);
                        var body = Encoding.UTF8.GetBytes(json);
                        var messageToSend = new BodyOutgoingMessage(body);
                        destination.SendAsync(messageToSend, NoReceiptBehavior.Default);
                    }

                }
                catch (Exception ex)
                {
                    LogService.LogError(ex);
                }
            });

        }

        public void PublishMachineStatus(MachineStatus status)
        {
            Task.Run(() =>
                {
                    //LogService.LogInfo("Publish Products");
                    try
                    {
                        var autoAck = client.SubscriptionBehaviors.AutoAcknowledge;
                        using (IDestination<IMessage> destination = client.GetDestination("/topic/machinestatus", autoAck))
                        {
                            //var json = JsonConvert.SerializeObject(tags);
                            var body = Encoding.UTF8.GetBytes(status.ToString());
                            var messageToSend = new BodyOutgoingMessage(body);
                            destination.SendAsync(messageToSend, NoReceiptBehavior.Default);
                        }

                    }
                    catch (Exception ex)
                    {
                        LogService.LogError(ex);
                    }
                });

        }

        public void MAPublishMachineStatus(MachineStatus status)
        {
            Task.Run(() =>
            {
                try
                {
                    using (var client = new StompClient("localhost", 61613))
                    {
                        var autoAck = client.SubscriptionBehaviors.AutoAcknowledge;
                        using (IDestination<IMessage> destination = client.GetDestination("/topic/machinestatus", autoAck))
                        {
                            //var json = JsonConvert.SerializeObject(tags);
                            var body = Encoding.UTF8.GetBytes(status.ToString());
                            var messageToSend = new BodyOutgoingMessage(body);
                            destination.SendAsync(messageToSend, NoReceiptBehavior.Default);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogService.LogError(ex);
                }
            });
        }

        public void PublishTransactionCompleted(object message)
        {
            Task.Run(() =>
                {
                    try
                    {
                        var autoAck = client.SubscriptionBehaviors.AutoAcknowledge;
                        using (IDestination<IMessage> destination = client.GetDestination("/topic/demo", autoAck))
                        {
                            //var json = JsonConvert.SerializeObject(tags);
                            var body = Encoding.UTF8.GetBytes(message.ToString());
                            var messageToSend = new BodyOutgoingMessage(body);
                            destination.SendAsync(messageToSend, NoReceiptBehavior.Default);
                        }
                    }
                    catch (Exception ex)
                    {
                        LogService.LogError(ex);
                    }
                });

        }

        public void SubCommand()
        {
            try
            {
                LogService.LogInfo("Stomp Sub");
                var autoAck = client.SubscriptionBehaviors.AutoAcknowledge;
                IDestination<IMessage> destination = client.GetDestination("/topic/command", autoAck);
                destination.IncommingMessages.Subscribe(WriteMessageId);
                LogService.LogInfo("Messages are written to console.");

                //using (IDestination<IMessage> destination = client.GetDestination("/topic/command", autoAck))
                //{

                //    using (destination.IncommingMessages.Subscribe(WriteMessageId))
                //    {
                //        LogService.LogInfo("Messages are written to console. Press any key to unsubscribe and exit.");
                //        Console.ReadKey(true);
                //        LogService.LogInfo("Stomp Unsub, try to Sub again");
                //        //SubCommand();
                //    }
                //}
            }
            catch (Exception ex)
            {
                LogService.LogError(ex);
            }
        }


        private void WriteMessageId(IMessage obj)
        {
            var body = obj.Body;
            var msg = Encoding.ASCII.GetString(body);
            // Console.WriteLine("Read : " + msg);
            OnCommandRev?.Invoke(msg);
        }
    }

    class BodyOutgoingMessage : IOutgoingMessage
    {
        private readonly byte[] _body;

        public byte[] Body
        {
            get { return _body; }
        }

        public IEnumerable<IHeader> Headers
        {
            get
            {
                return Enumerable.Empty<IHeader>();
            }
        }

        public BodyOutgoingMessage(byte[] body)
        {
            _body = body;
        }
    }

}
