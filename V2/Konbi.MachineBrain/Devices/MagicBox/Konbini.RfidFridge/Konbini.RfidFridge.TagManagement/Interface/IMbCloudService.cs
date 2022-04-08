using Konbini.RfidFridge.TagManagement.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Konbini.RfidFridge.TagManagement.Interface
{
    public interface IMbCloudService
    {
        List<MachineDTO.Machine> GetAllMachines();
        List<ProductDTO.Product> GetAllProducts();

        Task<bool> BulkInsertTags(BulkTagsDto input);
    }
}
