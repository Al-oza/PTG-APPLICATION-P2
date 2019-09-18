using System.Web.Mvc;

namespace PTGApplication.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        public ActionResult About()
        { return View(); }
        public ActionResult Index()
        { return View(); }
    }
}
