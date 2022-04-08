using Abp.AspNetCore.SignalR.Hubs;
using Abp.Auditing;
using Abp.RealTime;
using KonbiCloud.RFIDTable;
using KonbiCloud.Web.RFIDTable.SignalR.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KonbiCloud.Web.RFIDTable.SignalR
{
    public class RFIDTableHub : OnlineClientHubBase
    {
        public static int TableDeviceState; // 0 disconnected, 1 - connected and working, 2 - connected but comport is not ready.
        private readonly ITableManager tableManager;
        public RFIDTableHub(IOnlineClientManager onlineClientManager, IClientInfoProvider clientInfoProvider, ITableManager tableManager) : base(onlineClientManager, clientInfoProvider)
        {
            this.tableManager = tableManager;
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

        public async Task UpdateDishes(IEnumerable<DishInfo> dishes)
        {
           
            var tran = tableManager.ProcessTransaction(dishes.Select(el => new PlateReadingInput() { UID = el.UID, UType = el.UType }));

            // update customer with transaction info.
            await Clients.Group("CustomerUI").SendCoreAsync("updateTransactionInfo", new[] { tran });


            await Clients.Group("AdminUI").SendCoreAsync("updateDishes", new[] { dishes });
        }
        #region CustomerUI 
        public async  Task<SessionInfo> GetSessionInfo()
        {
            var x = await tableManager.GetSessionInfo();
            return x;
        }
        #endregion
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}
