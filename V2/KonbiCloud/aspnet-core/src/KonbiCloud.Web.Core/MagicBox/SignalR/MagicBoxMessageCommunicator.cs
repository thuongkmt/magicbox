using System.Threading.Tasks;
using Abp.Dependency;
using Abp.ObjectMapping;
using Castle.Core.Logging;
using KonbiCloud.Dto;
using KonbiCloud.SignalR;
using Microsoft.AspNetCore.SignalR;

namespace KonbiCloud.Web.MagicBox.SignalR
{
    public class MagicBoxMessageCommunicator : IMagicBoxMessageCommunicator, ITransientDependency
    {
        public ILogger Logger { get; set; }

        private readonly IObjectMapper _objectMapper;

        private readonly IHubContext<MagicBoxHub> _magicBoxHub;

        public MagicBoxMessageCommunicator(
                                            IObjectMapper objectMapper,
                                            IHubContext<MagicBoxHub> magicBoxHub)
        {
            _objectMapper = objectMapper;
            _magicBoxHub = magicBoxHub;
            Logger = NullLogger.Instance;
        }

        public async Task SendMessageToAllClient(GeneralMessage message)
        {
            var signalRClient = _magicBoxHub.Clients.All;

            if (signalRClient == null)
            {
                Logger.Debug("Can not get client from MagicBox hub!");
            }

            await signalRClient.SendAsync("MagicBoxMessage", _objectMapper.Map<MessageDto>(message));
        }
    }
}