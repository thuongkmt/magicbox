using System;
using System.Collections.Generic;
using System.Text;

namespace KonbiCloud.Features.Custom
{
    public interface IRfidTableFeatureChecker
    {
        void CheckRfidTableFeatures(int? tenantId);
    }
}
