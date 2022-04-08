using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Konbini.RfidFridge.TagManagement.DTO
{
    public class TagDTO
    {
        public string TagId { get; set; }

        public TagDTO(string tagId)
        {
            TagId = tagId;
        }
    }
}
