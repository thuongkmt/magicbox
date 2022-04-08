using Microsoft.AspNetCore.Mvc;
using KonbiCloud.Web.Controllers;

namespace KonbiCloud.Web.Public.Controllers
{
    public class AboutController : KonbiCloudControllerBase
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}