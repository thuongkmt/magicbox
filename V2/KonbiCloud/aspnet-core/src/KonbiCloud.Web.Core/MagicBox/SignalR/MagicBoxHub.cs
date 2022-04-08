using Abp.AspNetCore.SignalR.Hubs;
using Abp.Auditing;
using Abp.RealTime;
using System;
using System.Threading.Tasks;

namespace KonbiCloud.Web.MagicBox.SignalR
{
    public class MagicBoxHub : OnlineClientHubBase
    {
        public MagicBoxHub(IOnlineClientManager onlineClientManager,
                           IClientInfoProvider clientInfoProvider
                           ) : base(onlineClientManager, clientInfoProvider)
        {
        }

        public override Task OnConnectedAsync()
        {
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {

            Groups.RemoveFromGroupAsync(Context.ConnectionId, "TableDevice");
            Groups.RemoveFromGroupAsync(Context.ConnectionId, "PaymentDevice");
            Groups.RemoveFromGroupAsync(Context.ConnectionId, "CustomerUI");
            Groups.RemoveFromGroupAsync(Context.ConnectionId, "AdminUI");
            return base.OnDisconnectedAsync(exception);
        }
       
        public Task JoinGroup(string groupName)
        {
            return Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }
        public Task LeaveGroup(string groupName)
        {
            return Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}
