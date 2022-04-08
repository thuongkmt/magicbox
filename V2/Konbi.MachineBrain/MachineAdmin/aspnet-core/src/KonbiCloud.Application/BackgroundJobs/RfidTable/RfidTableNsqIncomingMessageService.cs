using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Threading.BackgroundWorkers;
using Abp.Threading.Timers;
using KonbiBrain.Common.Messages;
using KonbiBrain.Messages;
using KonbiCloud.RFIDTable;
using KonbiCloud.SignalR;
using Konbini.Messages.Commands.RFIDTable;
using Konbini.Messages.Enums;
using Newtonsoft.Json;
using NsqSharp;

namespace KonbiCloud.BackgroundJobs
{
    //public class RfidTableNsqIncomingMessageService : BaseNsqIncomingMessageService
    //{
    //    private readonly IMessageCommunicator messageCommunicator;
    //    private readonly IRfidTableSignalRMessageCommunicator rfidTableMessageCommunicator;
    //    private readonly ITableManager tableManager;

    //    public RfidTableNsqIncomingMessageService(AbpTimer timer, IMessageCommunicator messageCommunicator, ITableManager tableManager, IRfidTableSignalRMessageCommunicator rfidTableMessageCommunicator) : base(timer, NsqTopics.RFID_TABLE_RESPONSE_TOPIC)
    //    {
    //        this.messageCommunicator = messageCommunicator;
    //        this.tableManager = tableManager;
    //        this.rfidTableMessageCommunicator = rfidTableMessageCommunicator;

    //    }



    //    protected override async Task ProcessIncomingMessage(IMessage message)
    //    {
    //        var msg = Encoding.UTF8.GetString(message.Body);
    //        //LogService.NsqCommandLog.Information("KonbiMain incoming: " + msg);
    //        var obj = JsonConvert.DeserializeObject<UniversalCommands<object>>(msg);
    //        if (obj.Command == UniversalCommandConstants.RfidTableDetectPlates)
    //        {
    //            var mess = "{\"type\":\"RFIDTable_DetectedDisc\",\"data\":";
    //            mess += obj.CommandObject;
    //            mess += "}";// "]}";
    //            await messageCommunicator.SendTestMessageToAllClient(new GeneralMessage { Message = mess });

    //            var command = JsonConvert.DeserializeObject<UniversalCommands<DetectPlatesCommandPayload>>(msg);




    //            if (tableManager.OnSale)
    //            {

    //                // processing transaction and notify customer UI
    //                var tran = await tableManager.ProcessTransactionAsync(command.CommandObject.Plates.Select(el => new PlateReadingInput() { UID = el.UID, UType = el.UType }));

    //                await rfidTableMessageCommunicator.UpdateTransactionInfo(tran);
    //            }

    //            else
    //            {
    //                // notify Admin about detecting dishes
    //                await rfidTableMessageCommunicator.SendAdminDetectedPlates(command.CommandObject.Plates);

    //            }


    //            await messageCommunicator.SendTestMessageToAllClient(new GeneralMessage { Message = "mess" });
    //        }
    //    }

        //public void LogFailedMessage(IMessage message)
        //{

        //}

        //public void Dispose()
        //{
        //    consumer?.Stop();
        //}
    //}
}
