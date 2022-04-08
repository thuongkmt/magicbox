using System.Threading.Tasks;
using KonbiCloud.Security.Recaptcha;

namespace KonbiCloud.Tests.Web
{
    public class FakeRecaptchaValidator : IRecaptchaValidator
    {
        public Task ValidateAsync(string captchaResponse)
        {
            return Task.CompletedTask;
        }
    }
}
