using System;
using System.ComponentModel.DataAnnotations;
using Abp.Application.Services.Dto;
using Abp.AutoMapper;

namespace KonbiCloud.Tray.Dtos
{
    [AutoMapFrom(typeof(Plate.Tray))]
    public class TrayDto : EntityDto<Guid?>
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string Code { get; set; }

        public string Message { get; set; }
    }

    public class TrayRequest : PagedAndSortedResultRequestDto
    {
        public string NameFilter { get; set; }
        public string CodeFilter { get; set; }
    }
}