using Konbini.RfidFridge.Domain.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Konbini.RfidFridge.Service.Data
{
    public interface ITemperatureService
    {

        void Log(double temp);
    }
}
