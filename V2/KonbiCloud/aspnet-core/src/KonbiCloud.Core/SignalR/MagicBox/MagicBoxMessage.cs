using KonbiCloud.Enums;

namespace KonbiCloud.SignalR
{
    public class MagicBoxMessage : GeneralMessage
    {
        public System.Guid MachineId { get; set; }
        public MagicBoxMessageType MessageType { get; set; }
    }
}