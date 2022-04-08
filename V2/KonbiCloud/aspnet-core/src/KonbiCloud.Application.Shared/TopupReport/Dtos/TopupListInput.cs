using Abp.Application.Services.Dto;
using System;
using System.Collections.Generic;
using System.Text;

namespace KonbiCloud.TopupReport.Dtos
{
    public class TopupListInput: PagedAndSortedResultRequestDto
    {
        public string Keyword { get; set; }

        public Guid? MachineId { get; set; }

        public DateTime? StartTime { get; set; }

        public DateTime? EndTime { get; set; }
    }
}
