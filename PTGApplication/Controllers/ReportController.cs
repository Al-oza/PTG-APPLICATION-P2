using System.Web.Mvc;

namespace PTGApplication.Controllers
{
    public class ReportController : Controller
    {
        // GET: Report
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ExpiredDrugs()
        {
            return View();
        }

        public ActionResult Inventory()
        {
            return View();
        }
    }
}