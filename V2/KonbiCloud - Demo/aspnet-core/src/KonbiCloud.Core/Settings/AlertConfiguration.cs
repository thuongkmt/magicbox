using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;

namespace KonbiCloud.Settings
{
    public class AlertConfiguration : AuditedEntity<Guid>, IMustHaveTenant
    {
        [DisplayName("To email")]
        //[RegularExpression(@"^[a-z][a-z0-9_\.]{5,32}@[a-z0-9]{2,}(\.[a-z0-9]{2,5}){1,2}$", ErrorMessage = "Invalid Email Address")]
        [Required]
        public string ToEmail { get; set; }

      
        [DisplayName("When chilled machine temperature above")]
        [Required]
        public int WhenChilledMachineTemperatureAbove { get; set; }
      

        [DisplayName("When hot machine temperature below")]
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Please enter a value bigger than {1}")]
        public int WhenHotMachineTemperatureBelow { get; set; }

        [DisplayName("Send email when product expired date")]
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Please enter a value bigger than {1}")]
        public int SendEmailWhenProductExpiredDate { get; set; }

        [DisplayName("When stock bellow")]
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Please enter a value bigger than {1}")]
        public int WhenStockBellow { get; set; }

        public int TenantId { get; set; }

    }
}
