using System;
using System.Collections.Generic;
using System.Text;
using Abp.Application.Features;
using Abp.UI;
using KonbiCloud.Chat;

namespace KonbiCloud.Features.Custom
{
    public class RfidTableFeatureChecker : KonbiCloudDomainServiceBase, IRfidTableFeatureChecker
    {
        private readonly IFeatureChecker _featureChecker;

        public RfidTableFeatureChecker(IFeatureChecker featureChecker)
        {
            _featureChecker = featureChecker;
        }

        public void CheckRfidTableFeatures(int? tenantId)
        {
            if (tenantId.HasValue)
            {
                if (!_featureChecker.IsEnabled(tenantId.Value, AppFeatures.RFIDTableFeature))
                {
                    throw new UserFriendlyException("Rfid Table Feature isn't enable for this tenant");
                }
            }
        }
    }
}
