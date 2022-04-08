using Abp.Application.Services.Dto;
using KonbiCloud.SignalR;
using System;

namespace KonbiCloud.Dto
{
    public class MessageDto : EntityDto
    {
        public long UserId { get; set; }

        public int? TenantId { get; set; }

        public long TargetUserId { get; set; }

        public int? TargetTenantId { get; set; }

        public MessageSide Side { get; set; }

        public MessageReadState ReadState { get; set; }

        public MessageReadState ReceiverReadState { get; set; }

        public string Message { get; set; }
        
        public DateTime CreationTime { get; set; }

        public string SharedMessageId { get; set; }

        public MagicBoxMessageType MessageType { get; set; }
        public Guid MachineId { get; set; }
    }
}