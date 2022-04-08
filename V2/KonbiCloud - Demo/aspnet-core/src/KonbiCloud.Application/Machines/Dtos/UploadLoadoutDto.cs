using System;
using System.Collections.Generic;
using System.Text;

namespace KonbiCloud.Machines.Dtos
{
    public class UploadLoadoutDto
    {
        public string FileUrl { get; set; }
        public string FileContent { get; set; }
        public string MachineId { get; set; }
    }

    public class UploadLoadoutOutputDto
    {
        public bool IsSuccess{ get; set; }
        public string Message { get; set; }                
    }

    public class ImportInventoryItem
    {
        public string SKU { get; set; }
        public string Session { get; set; }
        public int Location { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
