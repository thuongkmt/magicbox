using Abp.Application.Services.Dto;
using KonbiCloud.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace KonbiCloud.Inventories.Dtos
{
    public class TopupDto : EntityDto<Guid>
    {

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int Total { get; set; }
        public int LeftOver { get; set; }

        public int Sold { get; set; }
        public int Error { get; set; }

        public string RestockerName { get; set; }

        public Guid MachineId { get; set; }


    }
}
