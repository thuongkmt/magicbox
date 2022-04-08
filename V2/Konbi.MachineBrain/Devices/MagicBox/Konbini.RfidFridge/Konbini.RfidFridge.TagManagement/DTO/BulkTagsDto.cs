using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Konbini.RfidFridge.TagManagement.DTO
{
    public class ListTag
    {
        public string name { get; set; }
        public string productId { get; set; }
    }

    public class BulkTagsDto
    {
        public IList<ListTag> listTags { get; set; }
        public int tenantId { get; set; }
    }
}
