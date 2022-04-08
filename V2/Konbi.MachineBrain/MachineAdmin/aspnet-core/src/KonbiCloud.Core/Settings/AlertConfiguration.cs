using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;

namespace KonbiCloud.Settings
{
    public class AlertConfiguration : AuditedEntity<Guid>, IMustHaveTenant
    {
        [DisplayName("To email")]
        //[RegularExpression(@"^[a-z][a-z0-9_\.]{5,32}@[a-z0-9]{2,}(\.[a-z0-9]{2,5}){1,2}$", ErrorMessage = "Invalid Email Address")]
        public string ToEmail { get; set; }

        [DisplayName("When chilled machine temperature above")]
        public int? WhenChilledMachineTemperatureAbove { get; set; }

        [DisplayName("When frozen machine temperature above")]
        public int? WhenFrozenMachineTemperatureAbove { get; set; }

        [DisplayName("When hot machine temperature below")]
        public int? WhenHotMachineTemperatureBelow { get; set; }

        [DisplayName("Send email when product expired date")]
        public int? SendEmailWhenProductExpiredDate { get; set; }

        [DisplayName("When stock bellow")]
        public int? WhenStockBellow { get; set; }

        public int TenantId { get; set; }

    }
}
