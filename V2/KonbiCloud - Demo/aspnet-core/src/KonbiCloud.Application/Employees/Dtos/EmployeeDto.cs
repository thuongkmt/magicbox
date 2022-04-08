using System;
using Abp.Application.Services.Dto;
using Abp.AutoMapper;

namespace KonbiCloud.Employees.Dtos
{
    [AutoMapFrom(typeof(Employee))]
    public class EmployeeDto : FullAuditedEntityDto<Guid>
    {
        public string Name { get; set; }
        public string CardId { get; set; }
        public double Quota { get; set; }
        public string Period { get; set; }
        public double Ordered { get; set; }
        public bool QuotaCash { get; set; }
        public double Price { get; set; }

        public int TenantId { get; set; }
    }

    public class EmployeeRequest : PagedAndSortedResultRequestDto
    {
        public string NameFilter { get; set; }
    }

    public class UploadEmployeeDto
    {
        public string FileContent { get; set; }
    }
}