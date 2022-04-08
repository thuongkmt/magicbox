using System;
using System.Collections.Generic;
using System.Text;

namespace KonbiCloud.PlateMenu.Dtos
{
    public class PlateMenuDayResult
    {
        public string SessionId { get; set; }
        public string SessionName { get; set; }
        public int TotalSetPrice { get; set; }
        public int TotalNoPrice { get; set; }
    }
}
