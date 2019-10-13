using System.Web.Mvc;

namespace PTGApplication.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
      
        public ActionResult Index()
        { return View(); }
    }
}
