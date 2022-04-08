using System.Threading.Tasks;
using Abp.EntityFrameworkCore;
using KonbiCloud.EntityFrameworkCore;
using KonbiCloud.EntityFrameworkCore.Repositories;

namespace KonbiCloud.MultiTenancy.Payments
{
    public class SubscriptionPaymentRepository : KonbiCloudRepositoryBase<SubscriptionPayment, long>, ISubscriptionPaymentRepository
    {
        public SubscriptionPaymentRepository(IDbContextProvider<KonbiCloudDbContext> dbContextProvider)
            : base(dbContextProvider)
        {
        }

        public async Task<SubscriptionPayment> UpdateByGatewayAndPaymentIdAsync(SubscriptionPaymentGatewayType gateway, string paymentId, int? tenantId, SubscriptionPaymentStatus status)
        {
            var payment = await SingleAsync(p => p.PaymentId == paymentId && p.Gateway == gateway);

            payment.Status = status;

            if (tenantId.HasValue)
            {
                payment.TenantId = tenantId.Value;
            }

            return payment;
        }

        public async Task<SubscriptionPayment> GetByGatewayAndPaymentIdAsync(SubscriptionPaymentGatewayType gateway, string paymentId)
        {
            return await SingleAsync(p => p.PaymentId == paymentId && p.Gateway == gateway);
        }
    }
}
