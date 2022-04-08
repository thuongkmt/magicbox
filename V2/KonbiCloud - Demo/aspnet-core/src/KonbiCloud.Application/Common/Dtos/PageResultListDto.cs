using Abp.Application.Services.Dto;
using System;
using System.Collections.Generic;
using System.Text;

namespace KonbiCloud.Common.Dtos
{
    public class PageResultListDto<T> : ListResultDto<T>
    {
        public PageResultListDto(IReadOnlyList<T> items, int totalCount = -1) : base(items)
        {
            TotalCount = totalCount;
        }

        public int TotalCount { get; set; }
    }
}
