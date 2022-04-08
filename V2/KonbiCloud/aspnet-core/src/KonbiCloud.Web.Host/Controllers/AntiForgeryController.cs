using Microsoft.AspNetCore.Antiforgery;

namespace KonbiCloud.Web.Controllers
{
    public class AntiForgeryController : KonbiCloudControllerBase
    {
        private readonly IAntiforgery _antiforgery;

        public AntiForgeryController(IAntiforgery antiforgery)
        {
            _antiforgery = antiforgery;
        }

        public void GetToken()
        {
            _antiforgery.SetCookieTokenAndHeader(HttpContext);
        }
    }
}
