using Abp.Notifications;
using KonbiCloud.Dto;

namespace KonbiCloud.Notifications.Dto
{
    public class GetUserNotificationsInput : PagedInputDto
    {
        public UserNotificationState? State { get; set; }
    }
}