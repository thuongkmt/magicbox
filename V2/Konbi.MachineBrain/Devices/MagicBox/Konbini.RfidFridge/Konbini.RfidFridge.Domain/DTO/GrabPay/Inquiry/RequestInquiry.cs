using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Konbini.RfidFridge.Domain.DTO
{
    public class RequestInquiry
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
        public string txType { get; set; }
        [Required]
        [StringLength(32)]
        public string partnerTxID { get; set; }        
        public string[] additionalInfo { get; set; }
    }
}
