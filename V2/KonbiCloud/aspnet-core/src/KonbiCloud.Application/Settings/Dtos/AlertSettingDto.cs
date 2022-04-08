using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using System;

namespace KonbiCloud.Settings.Dtos
{
    [AutoMap(typeof(AlertConfiguration))]
    public class AlertSettingDto : EntityDto<Guid?>
    {
        public string ToEmail { get; set; }

        public int? WhenChilledMachineTemperatureAbove { get; set; }

        public int? WhenFrozenMachineTemperatureAbove { get; set; }

        public int? WhenHotMachineTemperatureBelow { get; set; }

        public int? SendEmailWhenProductExpiredDate { get; set; }

        public int? WhenStockBellow { get; set; }
    }
}
