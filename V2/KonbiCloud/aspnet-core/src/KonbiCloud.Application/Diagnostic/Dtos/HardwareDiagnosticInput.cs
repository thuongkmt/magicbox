using System;
using Abp.Application.Services.Dto;

namespace KonbiCloud.Diagnostic.Dtos
{

    public class HardwareDiagnosticInput : IPagedAndSortedResultRequest
    {
        public int MaxResultCount { get; set; }
        public int SkipCount { get; set; }
        public string Sorting { get; set; } = "name";
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string MachineId { get; set; }
        public int? Element { get; set; }
    }
}
