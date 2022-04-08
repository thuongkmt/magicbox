using System.Threading.Tasks;

namespace KonbiCloud.Security.Recaptcha
{
    public interface IRecaptchaValidator
    {
        Task ValidateAsync(string captchaResponse);
    }
}