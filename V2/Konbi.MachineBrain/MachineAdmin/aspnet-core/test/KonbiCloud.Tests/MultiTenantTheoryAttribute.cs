using Xunit;

namespace KonbiCloud.Tests
{
    public sealed class MultiTenantTheoryAttribute : TheoryAttribute
    {
        public MultiTenantTheoryAttribute()
        {
            if (!KonbiCloudConsts.MultiTenancyEnabled)
            {
                Skip = "MultiTenancy is disabled.";
            }
        }
    }
}