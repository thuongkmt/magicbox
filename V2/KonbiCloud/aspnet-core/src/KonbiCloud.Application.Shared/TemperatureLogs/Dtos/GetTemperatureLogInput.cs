using System;

namespace KonbiCloud.TemperatureLogs.Dtos
{
    public class GetTemperatureLogInput
    {
        public string Filter { get; set; }
        public DateTime? DateFilter { get; set; }
    }
}
