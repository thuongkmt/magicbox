using Abp.Auditing;
using Microsoft.AspNetCore.Mvc;

namespace KonbiCloud.Web.Controllers
{
    public class HomeController : KonbiCloudControllerBase
    {
        [DisableAuditing]
        public IActionResult Index()
        {
            return Redirect("/swagger");
        }
    }
}
