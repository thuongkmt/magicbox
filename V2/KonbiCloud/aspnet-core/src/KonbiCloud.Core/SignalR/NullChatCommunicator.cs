using System.Collections.Generic;
using System.Threading.Tasks;
using Abp;
using Abp.RealTime;
using KonbiCloud.Friendships;

namespace KonbiCloud.SignalR
{
    public class NullMessageCommunicator : IMessageCommunicator
    {
        public async Task SendMessageToClient(IReadOnlyList<IOnlineClient> clients, GeneralMessage message)
        {
            await Task.CompletedTask;
        }

        public async Task SendTestMessageToClient(IReadOnlyList<IOnlineClient> clients, GeneralMessage message)
        {
            await Task.CompletedTask;
        }

        public Task SendTestMessageToAllClient(GeneralMessage message)
        {
            return Task.CompletedTask;
        }

        public async Task SendUserConnectionChangeToClients(IReadOnlyList<IOnlineClient> clients, UserIdentifier user, bool isConnected)
        {
            await Task.CompletedTask;
        }

        public async Task SendUserStateChangeToClients(IReadOnlyList<IOnlineClient> clients, UserIdentifier user, FriendshipState newState)
        {
            await Task.CompletedTask;
        }

        public async Task SendAllUnreadMessagesOfUserReadToClients(IReadOnlyList<IOnlineClient> clients, UserIdentifier user)
        {
            await Task.CompletedTask;
        }

        public async Task SendReadStateChangeToClients(IReadOnlyList<IOnlineClient> clients, UserIdentifier user)
        {
            await Task.CompletedTask;
        }
    }
}