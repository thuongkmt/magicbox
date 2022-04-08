using KonbiCloud.TopupReport.Dtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace KonbiCloud.Inventories.Dtos
{
    public class RestockInput: TopupDto
    {      
        /// <summary>
        /// 
        /// </summary>
        public List<InventoryDto> Inventory { get; set; }
    }
}
