using System;
using System.Collections.Generic;
using System.Text;

namespace KonbiCloud.SystemConfig.Dtos
{
    public class UpdateConfigInput
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public int TenantId { get; set; }
    }
}
