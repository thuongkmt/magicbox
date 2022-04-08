using System;
using System.Text;
using System.Threading.Tasks;
using Abp.Threading.Timers;
using KonbiBrain.Common.Messages;
using KonbiBrain.Messages;
using Newtonsoft.Json;
using NsqSharp;

namespace KonbiCloud.BackgroundJobs
{
    public class PaymentResponseNsqIncomingMessageService:BaseNsqIncomingMessageService
    {
        public PaymentResponseNsqIncomingMessageService(AbpTimer timer) : base(timer, NsqTopics.PAYMENT_RESPONSE_TOPIC)
        {
        }

        protected override  async  Task ProcessIncomingMessage(IMessage message)
        {
            var msg = Encoding.UTF8.GetString(message.Body);
            var cmd = JsonConvert.DeserializeObject<UniversalCommands>(msg);
            //if (cmd.Command == UniversalCommandConstants.MdbCashlessReponse)
            //{
                //todo publish signal command here
                throw new NotImplementedException();
            //}
        }
    }
}
