using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using Abp.Runtime.Session;
using KonbiCloud.Enums;

namespace KonbiCloud.Machines
{
    public class PlannedInventoryAppService : KonbiCloudAppServiceBase, IPlannedInventoryAppService
    {
        private int MACHINE_LOADOUT_ROW = 6;
        private int MACHINE_LOADOUT_COL = 10;
        private IRepository<Session,Guid> sessionRepository;

        public PlannedInventoryAppService(IRepository<Session, Guid>  _sessionRepository)
        {
            sessionRepository = _sessionRepository;
        }
        public async Task InitPlannedInventory(Machine machine)
        {
            //var sessions = await sessionRepository.GetAllListAsync(x => x.TenantId == AbpSession.GetTenantId());
            //if (sessions == null || sessions.Count == 0)
            //{
            //    //add new session
            //    var session = new Session()
            //    {
            //        Id = Guid.NewGuid(),
            //        Name = "A",
            //        //Type = SessionType.A,
            //        TenantId = AbpSession.GetTenantId(),
            //        //TenantId = 1
            //    };
            //    await sessionRepository.InsertAsync(session);
            //    var inventory = new PlannedSessionInventory()
            //    {
            //        Session = session,
            //        TenantId = AbpSession.GetTenantId(),
                 
            //        //TenantId = 1,
            //        Machine = machine
            //    };
            //    inventory.Items = CreateBlankLoadout(machine, inventory);
            //    machine.PlannedSessionInventories.Add(inventory);
            //}
            //else
            //{
            //    foreach (var session in sessions)
            //    {
            //        var inventory = new PlannedSessionInventory()
            //        {
            //            Session = session,
            //            TenantId = AbpSession.GetTenantId(),
            //        };
            //        inventory.Items = CreateBlankLoadout(machine, inventory);
            //        await sessionRepository.UpdateAsync(session);
            //        machine.PlannedSessionInventories.Add(inventory);
            //    }
            //}
            
        }

        public Task GetPlannedInventory(Guid machineId, Session session)
        {
            throw new NotImplementedException();
        }

        //private List<ItemInventory> CreateBlankLoadout(Machine machine,PlannedSessionInventory plannedSessionInventory)
        //{
        //    var itemInventories = new List<ItemInventory>();
        //    // Create blank inventory items
        //    var totalItems = (MACHINE_LOADOUT_ROW * MACHINE_LOADOUT_COL);
        //    for (int i = 0; i < totalItems; i++)
        //    {
        //        var item = new ItemInventory()
        //        {
        //            LocationCode = (((i + 10) / 10) * 100) + (i % 10 + 1),
        //            Machine = machine,
        //            PlannedSessionInventory = plannedSessionInventory,
        //            IsBlank = true,
        //            TenantId = AbpSession.GetTenantId()
        //        };
        //        itemInventories.Add(item);
        //    }
        //    return itemInventories;
        //}
    }
}
