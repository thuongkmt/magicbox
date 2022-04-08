using System;

namespace KonbiCloud.Messages
{
    public class UniversalCommands
    {
        public static readonly Guid RFIDTable_DetectedDisc = Guid.Parse("C2CC55A6-E61A-4F23-A260-2C6EA5EEC17D");
        public static readonly Guid RFIDTable_RemovedDisc = Guid.Parse("9E37DE21-1949-447E-AD87-8933D13AD4CA");

        public Guid CommandId { get; set; }

        public UniversalCommands(Guid id)
        {
            this.CommandId = id;
        }
        public dynamic CommandObject { get; set; }
    }
}
