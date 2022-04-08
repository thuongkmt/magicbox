namespace Konbini.RfidFridge.Service.Data
{
    using System.Collections.Generic;

    using Konbini.RfidFridge.Domain.Entities;
    using Konbini.RfidFridge.Service.Base;
    using System.Threading.Tasks;
    using Konbini.RfidFridge.Domain.DTO;

    public interface IInventoryService : IEntityService<Inventory>
    {
        Task<List<InventoryDto>> GetAllFromWebApi();

        Task Delete(string id);

        void RemoveByIds(List<string> ids);
    }
}
