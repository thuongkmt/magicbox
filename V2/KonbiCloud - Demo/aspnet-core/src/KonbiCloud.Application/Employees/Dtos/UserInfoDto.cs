using System;
using System.Collections.Generic;
using System.Text;

namespace KonbiCloud.Employees.Dtos
{
    public class UserInfoDto
    {
        public string UserId { get; set; }
        public string Name { get; set; }
        public string IdCard { get; set; }
        public string Department { get; set; }
    }

    public class DealDataSyncHandDto
    {
     
        public DateTime Time { get; set; }
        public string IdCard { get; set; }
        public double ItemPrice { get; set; }
        public int Num { get; set; }
        public string MachineId { get; set; }
    }
}
