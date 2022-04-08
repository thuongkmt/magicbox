using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Konbini.RfidFridge.Domain.DTO
{
    public class TemperatureDto
    {
        public double Temp { get; set; }
        public int Index { get; set; }
        public long LastUpdate { get; set; }

        public TemperatureDto(List<double> tmp, int index)
        {
            Temp = tmp[index];
            Index = index;
            LastUpdate = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }

        public override  string ToString()
        {
            return $"Temp: {Temp}°C | Index: {Index}";
        }
    }
}
