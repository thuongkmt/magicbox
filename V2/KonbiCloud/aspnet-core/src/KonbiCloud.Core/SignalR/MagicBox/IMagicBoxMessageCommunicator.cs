using System.Threading.Tasks;

namespace KonbiCloud.SignalR
{
    public interface IMagicBoxMessageCommunicator
    {
        Task SendMessageToAllClient(GeneralMessage message);
    }
}
