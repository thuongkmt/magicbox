using System;
using System.Collections.Generic;
using System.Text;

namespace KonbiCloud.LinePay.Dtos
{
    public class LinePayFinishDto
    {
        public string regkey { get; set; }
        public Int64 transactionId { get; set; }
        public int amount { get; set; }
        public string productName { get; set; }
    }
}
