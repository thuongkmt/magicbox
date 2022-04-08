using Abp.Application.Services.Dto;
using KonbiCloud.Enums;
using System;

namespace KonbiCloud.TopupReport.Dtos
{
    public class TopupListDto: EntityDto<Guid>
    {
        public Guid MachineId { get; set; }

        public string MachineName { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        public int Total { get; set; }

        public int Sold { get; set; }

        public int Errors { get; set; }

        public decimal SalesAmount { get; set; }

        public string RestockerName { get; set; }

        public TopupTypeEnum Type { get; set; }
    }
}
