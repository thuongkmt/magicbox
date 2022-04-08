using Abp.Dependency;
using System;
using System.Collections.Generic;
using System.Text;

namespace KonbiCloud.Common
{
    public interface IDetailLogService : ISingletonDependency
    {
        void Log(string message);
    }
}
