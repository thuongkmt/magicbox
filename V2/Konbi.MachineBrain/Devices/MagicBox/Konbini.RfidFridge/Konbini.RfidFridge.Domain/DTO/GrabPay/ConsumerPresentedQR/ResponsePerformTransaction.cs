using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Konbini.RfidFridge.Domain.DTO
{
    public class ResponsePerformTransaction : LogicError
    {
        [Required]
        [StringLength(32)]
        public string msgID { get; set; }
        [Required]
        [StringLength(32)]
        public string txID { get; set; }
        public string status { get; set; }
        [Required]
        [StringLength(3)]
        public string currency { get; set; }
        [Required]
        public Int64 amount { get; set; }
        [Required]
        public Int64 updated { get; set; }
        public string errMsg { get; set; }
        public string additionalInfo { get; set; }
    }
}
