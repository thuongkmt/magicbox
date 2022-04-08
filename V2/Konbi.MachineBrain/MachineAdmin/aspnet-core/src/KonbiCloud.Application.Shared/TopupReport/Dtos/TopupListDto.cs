using Abp.Application.Services.Dto;
using System;
using System.Collections.Generic;
using System.Text;

namespace KonbiCloud.TopupReport.Dtos
{
    public class TopupListDto: EntityDto<Guid>
    {
        public string MachineName { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        public int Total { get; set; }

        public int Sold { get; set; }

        public int Errors { get; set; }

        public decimal SalesAmount { get; set; }
    }
}
