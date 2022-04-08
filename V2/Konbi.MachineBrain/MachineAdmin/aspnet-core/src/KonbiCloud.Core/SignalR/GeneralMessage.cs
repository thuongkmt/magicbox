using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Abp.Timing;

namespace KonbiCloud.SignalR
{
    [Table("GeneralMessages")]
    public class GeneralMessage : Entity<long>, IHasCreationTime, IMayHaveTenant
    {
        public const int MaxMessageLength = 4 * 1024; //4KB

        public long UserId { get; set; }

        public int? TenantId { get; set; }

        public long TargetUserId { get; set; }

        public int? TargetTenantId { get; set; }

        [Required]
        [StringLength(MaxMessageLength)]
        public string Message { get; set; }

        public DateTime CreationTime { get; set; }

        public MessageSide Side { get; set; }

        public MessageReadState ReadState { get; private set; }

        public MessageReadState ReceiverReadState { get; private set; }

        public Guid? SharedMessageId { get; set; }

      

        public GeneralMessage(
            UserIdentifier user,
            UserIdentifier targetUser,
            MessageSide side,
            string message,
            MessageReadState readState,
            Guid sharedMessageId,
           MessageReadState receiverReadState)
        {
            UserId = user.UserId;
            TenantId = user.TenantId;
            TargetUserId = targetUser.UserId;
            TargetTenantId = targetUser.TenantId;
            Message = message;
            Side = side;
            ReadState = readState;
            SharedMessageId = sharedMessageId;
            ReceiverReadState = receiverReadState;

            CreationTime = Clock.Now;
        }

        public void ChangeReadState(MessageReadState newState)
        {
            ReadState = newState;
        }

        public GeneralMessage()
        {

        }

        public void ChangeReceiverReadState(MessageReadState newState)
        {
            ReceiverReadState = newState;
        }
    }
}
