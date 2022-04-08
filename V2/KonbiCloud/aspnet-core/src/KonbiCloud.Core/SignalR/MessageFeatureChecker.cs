using Abp.Application.Features;
using Abp.UI;
using KonbiCloud.Features;

namespace KonbiCloud.SignalR
{
    public class MessageFeatureChecker : KonbiCloudDomainServiceBase, IMessageFeatureChecker
    {
        private readonly IFeatureChecker _featureChecker;

        public MessageFeatureChecker(
            IFeatureChecker featureChecker
        )
        {
            _featureChecker = featureChecker;
        }

        public void CheckMessageFeatures(int? sourceTenantId, int? targetTenantId)
        {
            CheckMessageFeaturesInternal(sourceTenantId, targetTenantId, MessageSide.Sender);
            CheckMessageFeaturesInternal(targetTenantId, sourceTenantId, MessageSide.Receiver);
        }

        private void CheckMessageFeaturesInternal(int? sourceTenantId, int? targetTenantId, MessageSide side)
        {
            var localizationPosfix = side == MessageSide.Sender ? "ForSender" : "ForReceiver";
            if (sourceTenantId.HasValue)
            {
                if (!_featureChecker.IsEnabled(sourceTenantId.Value, AppFeatures.MessageFeature))
                {
                    throw new UserFriendlyException(L("MessageFeatureIsNotEnabled" + localizationPosfix));
                }

                if (targetTenantId.HasValue)
                {
                    if (sourceTenantId == targetTenantId)
                    {
                        return;
                    }

                    if (!_featureChecker.IsEnabled(sourceTenantId.Value, AppFeatures.MessageFeature))
                    {
                        throw new UserFriendlyException(L("TenantToTenantMessageFeatureIsNotEnabled" + localizationPosfix));
                    }
                }
                else
                {
                    if (!_featureChecker.IsEnabled(sourceTenantId.Value, AppFeatures.MessageFeature))
                    {
                        throw new UserFriendlyException(L("TenantToHostMessageFeatureIsNotEnabled" + localizationPosfix));
                    }
                }
            }
            else
            {
                if (targetTenantId.HasValue)
                {
                    if (!_featureChecker.IsEnabled(targetTenantId.Value, AppFeatures.MessageFeature))
                    {
                        throw new UserFriendlyException(L("TenantToHostMessageFeatureIsNotEnabled" + (side == MessageSide.Sender ? "ForReceiver" : "ForSender")));
                    }
                }
            }
        }
    }
}