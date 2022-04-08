using System;
using System.Threading.Tasks;
using Abp;
using Abp.Domain.Services;

namespace KonbiCloud.SignalR
{
    public interface IMessageManager : IDomainService
    {
        Task SendMessageAsync(UserIdentifier sender, UserIdentifier receiver, string message, string senderTenancyName, string senderUserName, Guid? senderProfilePictureId);

        long Save(GeneralMessage message);

        int GetUnreadMessageCount(UserIdentifier userIdentifier, UserIdentifier sender);

        Task<GeneralMessage> FindMessageAsync(int id, long userId);
    }
}
