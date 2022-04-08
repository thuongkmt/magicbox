//using System.Collections.Generic;
//using System.Threading.Tasks;
//using Abp;
//using Abp.Dependency;
//using Abp.ObjectMapping;
//using Abp.RealTime;
//using Castle.Core.Logging;
//using KonbiCloud.Chat;
//using KonbiCloud.Chat.Dto;
//using KonbiCloud.Dto;
//using KonbiCloud.Friendships;
//using KonbiCloud.Friendships.Dto;
//using KonbiCloud.SignalR;
//using KonbiCloud.Web.Chat.SignalR;
//using KonbiCloud.Web.RFIDTable.SignalR;
//using Microsoft.AspNetCore.SignalR;

//namespace KonbiCloud.Web.SignalR
//{
//    public class SignalRMessageCommunicator : IMessageCommunicator, ITransientDependency
//    {
//        /// <summary>
//        /// Reference to the logger.
//        /// </summary>
//        public ILogger Logger { get; set; }

//        private readonly IObjectMapper _objectMapper;

//        private readonly IHubContext<RfidTableMessageHub> _messageHub;
//        private readonly IHubContext<RFIDTableHub> _messageRfidHub;

//        public SignalRMessageCommunicator(
//            IObjectMapper objectMapper,
//            IHubContext<RfidTableMessageHub> messageHub, IHubContext<RFIDTableHub> messageRfidHub)
//        {
//            _objectMapper = objectMapper;
//            _messageHub = messageHub;
//            _messageRfidHub = messageRfidHub;
//            Logger = NullLogger.Instance;
//        }

//        public async Task SendMessageToClient(IReadOnlyList<IOnlineClient> clients, GeneralMessage message)
//        {
//            foreach (var client in clients)
//            {
//                var signalRClient = GetSignalRClientOrNull(client);
//                if (signalRClient == null)
//                {
//                    return;
//                }

//                await signalRClient.SendAsync("getMessage", _objectMapper.Map<MessageDto>(message));
//            }
//        }

//        public async Task SendRfidTableMessageToClient(IReadOnlyList<IOnlineClient> clients, GeneralMessage message)
//        {
//            foreach (var client in clients)
//            {
//                var signalRClient = GetSignalRClientOrNull(client);
//                if (signalRClient == null)
//                {
//                    return;
//                }

//                await signalRClient.SendAsync("getRfidTableMessage", _objectMapper.Map<MessageDto>(message));
//            }
//        }


//        public async Task SendRfidTableMessageToAllClient(GeneralMessage message)
//        {
//            var signalRClient = _messageHub.Clients.All;
//            if (signalRClient == null)
//            {
//                Logger.Debug("Can not get chat user  from SignalR hub!");

//            }
//            await signalRClient.SendAsync("getRfidTableMessage", _objectMapper.Map<MessageDto>(message));
//        }




//        public async Task SendUserConnectionChangeToClients(IReadOnlyList<IOnlineClient> clients, UserIdentifier user, bool isConnected)
//        {
//            foreach (var client in clients)
//            {
//                var signalRClient = GetSignalRClientOrNull(client);
//                if (signalRClient == null)
//                {
//                    continue;
//                }

//                await signalRClient.SendAsync("getUserConnectNotification", user, isConnected);
//            }
//        }

//        public async Task SendUserStateChangeToClients(IReadOnlyList<IOnlineClient> clients, UserIdentifier user, FriendshipState newState)
//        {
//            foreach (var client in clients)
//            {
//                var signalRClient = GetSignalRClientOrNull(client);
//                if (signalRClient == null)
//                {
//                    continue;
//                }

//                await signalRClient.SendAsync("getUserStateChange", user, newState);
//            }
//        }

//        public async Task SendAllUnreadMessagesOfUserReadToClients(IReadOnlyList<IOnlineClient> clients, UserIdentifier user)
//        {
//            foreach (var client in clients)
//            {
//                var signalRClient = GetSignalRClientOrNull(client);
//                if (signalRClient == null)
//                {
//                    continue;
//                }

//                await signalRClient.SendAsync("getallUnreadMessagesOfUserRead", user);
//            }
//        }

//        public async Task SendReadStateChangeToClients(IReadOnlyList<IOnlineClient> clients, UserIdentifier user)
//        {
//            foreach (var client in clients)
//            {
//                var signalRClient = GetSignalRClientOrNull(client);
//                if (signalRClient == null)
//                {
//                    continue;
//                }

//                await signalRClient.SendAsync("getReadStateChange", user);
//            }
//        }

//        private IClientProxy GetSignalRClientOrNull(IOnlineClient client)
//        {
//            var signalRClient = _messageHub.Clients.Client(client.ConnectionId);
//            if (signalRClient == null)
//            {
//                Logger.Debug("Can not get chat user " + client.UserId + " from SignalR hub!");
//                return null;
//            }

//            return signalRClient;
//        }
//    }
//}