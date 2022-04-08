using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Konbini.RfidFridge.Domain.DTO
{
    public class RequestPerformTransaction
    {
        [Required]
        [StringLength(32)]
        public string msgID { get; set; }
        [Required]
        [StringLength(36)]
        public string grabID { get; set; }
        [Required]
        public string terminalID { get; set; }
        [Required]
        [StringLength(3)]
        public string currency { get; set; }
        [Required]
        public Int64 amount { get; set; }
        [Required]
        [StringLength(32)]
        public string partnerTxID { get; set; }
        [Required]
        [StringLength(18)]
        public string code { get; set; }
        public string[] additionalInfo { get; set; }
    }
}
