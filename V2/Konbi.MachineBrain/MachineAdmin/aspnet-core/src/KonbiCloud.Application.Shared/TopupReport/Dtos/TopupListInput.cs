using Abp.Application.Services.Dto;
using System;

namespace KonbiCloud.TopupReport.Dtos
{
    public class TopupListInput: PagedAndSortedResultRequestDto
    {
        public string Keyword { get; set; }

        public DateTime? StartTime { get; set; }

        public DateTime? EndTime { get; set; }
    }
}
