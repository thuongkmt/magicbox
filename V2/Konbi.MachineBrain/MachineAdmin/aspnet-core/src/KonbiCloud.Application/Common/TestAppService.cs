using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Abp.Configuration;
using KonbiCloud.Configuration;
using KonbiCloud.DeviceSettings;
using KonbiCloud.Enums;
using KonbiCloud.Machines.Dtos;
using Konbini.Messages;
using Konbini.Messages.Enums;
using Konbini.Messages.Services;

namespace KonbiCloud.Common
{
    public class TestAppService    : KonbiCloudAppServiceBase
    {
        private readonly ISendMessageToCloudService _sendMessageToCloudService;
        private readonly IBillAcceptorHanlderService _billAcceptorHanlderService;
        private readonly string _machineId;

        public TestAppService(ISendMessageToCloudService sendMessageToCloudService,ISettingManager settingManager,IBillAcceptorHanlderService billAcceptorHanlderService)
        {
            _sendMessageToCloudService = sendMessageToCloudService;
            _machineId = settingManager.GetSettingValue(AppSettingNames.MachineId);
            _billAcceptorHanlderService = billAcceptorHanlderService;
        }

        public object SendTestRabbitMq()
        {
            var obj = new KeyValueMessage()
            {
                Key = MessageKeys.TestKey,
                MachineId =Guid.Parse(_machineId),
                Value = "Hello"
            };
            _sendMessageToCloudService.SendQueuedMsgToCloud(obj);
            return obj;

        }

        public async Task TestBillAcceptor(int cmd=0)
        {
            if(cmd==0) _billAcceptorHanlderService.Reset();
            if(cmd==1)_billAcceptorHanlderService.Enable();
            if(cmd==2)_billAcceptorHanlderService.Disable();
        }

        public async Task<KeyValueMessage> SyncMachineStatus()
        {
            var data = new MachineStatusDto
            {
                  State = DoorState.DOOR_CLOSED,
                  DoorLastClosed = DateTime.Now.AddMinutes(-10),
                  Temperature = 5,
                  MachineId = Guid.NewGuid().ToString(),
                  Uptime = 60 * 60 * 24,
                  Cpu = 15,
                  Memory = 14,
                  Hdd = 30,
                  PaymentState = PaymentState.ActivatingPayment,
                  TopupTime = DateTime.Now.AddHours(-1),
                  LastTagPutIn = "11111111111111111",
                  LastTagTakenOut = "2222222222222"
            };

            var kv = new KeyValueMessage()
            {
                Key = MessageKeys.MachineStatus,
                MachineId = Guid.Parse("8414bcfd-f716-4d72-a1e2-1af9673c3e6a"),
                Value = data,
            };
            _sendMessageToCloudService.SendMsgToCloud(kv);

            //todo: send mass of messages and simulate multiple machines

            return kv;
        }

        public async Task<KeyValueMessage> SyncTags()
        {
            var data =new List<KeyValuePair<string, Guid>>         //tagid, product guid pair
                {
                    new KeyValuePair<string, Guid>("tag01_HHA",Guid.Parse("08d6d04b-c130-01b4-e73e-ca9129448fac")),
                    new KeyValuePair<string, Guid>("tag02",Guid.Parse("08d6d04b-b950-ae1e-c716-8a43a35ff7fc")),
                    new KeyValuePair<string, Guid>("tag03",Guid.Parse("08d6d04b-b950-ae1e-c716-8a43a35ff7fc")),
                    new KeyValuePair<string, Guid>("tag04",Guid.Parse("08d6d04b-b950-ae1e-c716-8a43a35ff7fc")),
                    new KeyValuePair<string, Guid>("tag05",Guid.Parse("08d6d04b-b950-ae1e-c716-8a43a35ff7fc")),
                    new KeyValuePair<string, Guid>("tag06",Guid.Parse("08d6d04b-b950-ae1e-c716-8a43a35ff7fc")),
                    new KeyValuePair<string, Guid>("tag07",Guid.Parse("08d6d04b-b950-ae1e-c716-8a43a35ff7fc")),
                    new KeyValuePair<string, Guid>("tag08",Guid.Parse("08d6d04b-b950-ae1e-c716-8a43a35ff7fc")),
                    new KeyValuePair<string, Guid>("tag09",Guid.Parse("08d6d04b-b950-ae1e-c716-8a43a35ff7fc")),
                    new KeyValuePair<string, Guid>("tag10",Guid.Parse("08d6d04b-b950-ae1e-c716-8a43a35ff7fc")),
                    new KeyValuePair<string, Guid>("tag11",Guid.Parse("08d6d04b-b950-ae1e-c716-8a43a35ff7fc")),
                    new KeyValuePair<string, Guid>("tag12",Guid.Parse("08d6d04b-b950-ae1e-c716-8a43a35ff7fc")),
                    new KeyValuePair<string, Guid>("tag13",Guid.Parse("08d6d04b-b950-ae1e-c716-8a43a35ff7fc")),
                    new KeyValuePair<string, Guid>("tag14",Guid.Parse("08d6d04b-b950-ae1e-c716-8a43a35ff7fc")),
                    new KeyValuePair<string, Guid>("tag05",Guid.Parse("08d6d04b-b950-ae1e-c716-8a43a35ff7fc")),
                    new KeyValuePair<string, Guid>("tag05",Guid.Parse("08d6d04b-b950-ae1e-c716-8a43a35ff7fc")),
                    new KeyValuePair<string, Guid>("tag05",Guid.Parse("08d6d04b-b950-ae1e-c716-8a43a35ff7fc")),
                    new KeyValuePair<string, Guid>("tag05",Guid.Parse("08d6d04b-b950-ae1e-c716-8a43a35ff7fc")),
                    new KeyValuePair<string, Guid>("tag05",Guid.Parse("08d6d04b-b950-ae1e-c716-8a43a35ff7fc")),
                    new KeyValuePair<string, Guid>("tag05",Guid.Parse("08d6d04b-b950-ae1e-c716-8a43a35ff7fc")),
                    new KeyValuePair<string, Guid>("tag05",Guid.Parse("08d6d04b-b950-ae1e-c716-8a43a35ff7fc")),
                    new KeyValuePair<string, Guid>("tag05",Guid.Parse("08d6d04b-b950-ae1e-c716-8a43a35ff7fc")),
                    new KeyValuePair<string, Guid>("tag05",Guid.Parse("08d6d04b-b950-ae1e-c716-8a43a35ff7fc")),
                    new KeyValuePair<string, Guid>("tag05",Guid.Parse("08d6d04b-b950-ae1e-c716-8a43a35ff7fc")),

                }
                ;
            
            var kv = new KeyValueMessage()
            {   
          
               Key = MessageKeys.ProductRfidTags,
               MachineId = Guid.Parse("8414bcfd-f716-4d72-a1e2-1af9673c3e6a"),
               Value = new
               {
                   Price=10,
                   TagProductMaps = data
               }
            };
            _sendMessageToCloudService.SendQueuedMsgToCloud(kv);
            return kv;
        }

        public async Task<KeyValueMessage> SyncTemperatureLogs()
        {
            var kv = new KeyValueMessage()
            {

                Key = MessageKeys.TemperatureLogs,
                MachineId = Guid.Parse("8414bcfd-f716-4d72-a1e2-1af9673c3e6a"),
                Value = 68//temperature
            };

            _sendMessageToCloudService.SendMsgToCloud(kv);
            return kv;
        }


        public async Task<KeyValueMessage> SynProductTagsRealtime()
        {
            var kv = new KeyValueMessage()
            {

                Key = MessageKeys.ProductRfidTagsRealtime,
                MachineId = Guid.Parse("ad76de49-47d2-fb93-37ea-f08d40dbe152"),
                Value=new List<KeyValuePair<string, Guid>> {
                    new KeyValuePair<string, Guid>("tag77",Guid.NewGuid()),
                    new KeyValuePair<string, Guid>("tag88", Guid.NewGuid()),
                    new KeyValuePair<string, Guid>("tag99", Guid.NewGuid()),
                    new KeyValuePair<string, Guid>("tag100", Guid.NewGuid())}
            };

            _sendMessageToCloudService.SendMsgToCloud(kv);
            return kv;
        }
    }
}
