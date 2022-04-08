using Microsoft.AspNetCore.Mvc;
using KonbiCloud.Web.Controllers;

namespace KonbiCloud.Web.Public.Controllers
{
    public class HomeController : KonbiCloudControllerBase
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}