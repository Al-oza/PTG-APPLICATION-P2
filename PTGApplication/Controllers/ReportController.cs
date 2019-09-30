using System.Web.Mvc;

namespace PTGApplication.Controllers
{
    [Authorize]
    public class ReportController : Controller
    {
        public ActionResult About()
        { return View(); }
      
        public ActionResult Index()
        { return View(); }
    }
}
