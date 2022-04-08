using System;
using System.Collections.Generic;

namespace KonbiCloud.Inventories.Dtos
{
    public class GetCurrentInventoryInput : GetAllInventoriesInput
    {
		public IList<Guid> MachinesFilter { get; set; }
		 
    }
}