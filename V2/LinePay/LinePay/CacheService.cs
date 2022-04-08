using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LinePay
{
    public static class CacheService
    {
        public static Dictionary<Int64, object> Cache = new Dictionary<long, object>();
    }
}
