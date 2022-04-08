using System;
using System.Collections.Generic;
using System.Text;

namespace Konbini.RfidFridge.Domain.DTO
{
    public class DiskDriver
    {
        public string Name { get; set; }
        public decimal TotalSize { get; set; }
        public decimal AvaibleSize { get; set; }
    }
}
