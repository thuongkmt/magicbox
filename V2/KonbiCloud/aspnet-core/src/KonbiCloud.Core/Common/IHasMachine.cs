using KonbiCloud.Machines;
using System;
using System.Collections.Generic;
using System.Text;

namespace KonbiCloud.Common
{
    public interface IHasMachine
    {
        Machine Machine { get; set; }
    }
}
