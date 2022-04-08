using Abp.Application.Services.Dto;
using System;
using System.Collections.Generic;
using System.Text;

namespace KonbiCloud.Machines.Dtos
{
    public class MachineInputListDto : IPagedAndSortedResultRequest
    {       
        public int MaxResultCount { get; set; }
        public int SkipCount { get; set; }
        public string Sorting { get; set; }        
    }
}
