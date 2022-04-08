namespace KonbiCloud.SignalR
{
    public interface IMessageFeatureChecker
    {
        void CheckMessageFeatures(int? sourceTenantId, int? targetTenantId);
    }
}
