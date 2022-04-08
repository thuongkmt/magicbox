using Common.TruePayment;
using System.Threading.Tasks;

namespace KonbiCloud.Payments
{
    public interface ITruePaymentService
    {
        Task<OtpResponse> AuthenticateUser(AccountRequest input);
        Task<TokenResponse> SubmitOtp(TokenRequest input);
    }
}
