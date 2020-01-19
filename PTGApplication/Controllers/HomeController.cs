using System.Web.Mvc;

namespace PTGApplication.Controllers
{
    /// <summary>
    /// Controller for Main/Home Page
    /// </summary>
    [Authorize]
    public class HomeController : Controller
    { 
        /// <summary>
        /// Gets Home Page
        /// </summary>
        /// <returns>Home Page</returns>
        public ActionResult Index()
        { return View(); }
    }
}
