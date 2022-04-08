using System.ComponentModel.DataAnnotations;

namespace Konbini.RfidFridge.Domain.DTO
{
    public class ResponseCreateOrder
    {
        [Required]
        [StringLength(32)]
        public string msgID { get; set; }
        [Required]
        public string qrcode { get; set; }
        [Required]
        [StringLength(32)]
        public string txID { get; set; }
    }
}
