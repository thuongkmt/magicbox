using System;
using System.Collections.Generic;
using System.Text;

namespace KonbiCloud.Inventories.Dtos
{
    public class MachineInventoryCacheDto
    {
        public Guid MachineId { get; set; }
        public List<TagProductDto> Tags { get; set; }
        public DateTime? LastUpdated { get; set; }
    }
    public class TagProductDto
    {
        public string Tag { get; set; }
        public string ProductName { get; set; }
    }
}
