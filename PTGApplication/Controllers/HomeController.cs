using System.Web.Mvc;

namespace PTGApplication.Controllers
{
    [Authorize]
    /// <summary>
    /// Controller for Main/Home Page
    /// </summary>
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
