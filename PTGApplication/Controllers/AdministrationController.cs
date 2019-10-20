using PTGApplication.Models;
using System.Web.Mvc;

namespace PTGApplication.Controllers
{
    public class AdministrationController : Controller
    {
        // GET: Administration
        public ActionResult Index()
        {
            return View();
        }

        // GET: Administration/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Administration/Create
        [HttpPost]
        public ActionResult Create(RegisterViewModel collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Administration/Modify/5
        public ActionResult Modify(int id)
        {
            return View();
        }

        // POST: Administration/Modify/5
        [HttpPost]
        public ActionResult Modify(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Administration/Remove/5
        public ActionResult Remove(int id)
        {
            return View();
        }

        // POST: Administration/Remove/5
        [HttpPost]
        public ActionResult Remove(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}
