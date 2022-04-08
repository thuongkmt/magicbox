using System.Collections.Generic;
using System.Threading.Tasks;
using Abp;
using Abp.RealTime;
using KonbiCloud.Friendships;

namespace KonbiCloud.SignalR
{
    public interface IMessageCommunicator
    {
        Task SendMessageToClient(IReadOnlyList<IOnlineClient> clients, GeneralMessage message);
        Task SendTestMessageToClient(IReadOnlyList<IOnlineClient> clients, GeneralMessage message);
        Task SendTestMessageToAllClient(GeneralMessage message);

        Task SendUserConnectionChangeToClients(IReadOnlyList<IOnlineClient> clients, UserIdentifier user, bool isConnected);

        Task SendUserStateChangeToClients(IReadOnlyList<IOnlineClient> clients, UserIdentifier user, FriendshipState newState);

        Task SendAllUnreadMessagesOfUserReadToClients(IReadOnlyList<IOnlineClient> clients, UserIdentifier user);

        Task SendReadStateChangeToClients(IReadOnlyList<IOnlineClient> onlineFriendClients, UserIdentifier user);
    }
}
