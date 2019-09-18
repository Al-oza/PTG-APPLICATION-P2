using System.Web.Mvc;

namespace PTGApplication.Controllers
{
    public class PharmacyController : Controller
    {
        // GET: Pharmacy
        public ActionResult Home()
        { return View(); }

        // GET: Pharmacy/Details/5
        public ActionResult Details(int id)
        { return View(); }

        // GET: Pharmacy/Create
        public ActionResult Create()
        { return View(); }

        // POST: Pharmacy/Create
        [HttpPost]
        public ActionResult Create(FormCollection collection)
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

        // GET: Pharmacy/Edit/5
        public ActionResult Edit(int id)
        { return View(); }

        // POST: Pharmacy/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
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

        // GET: Pharmacy/Delete/5
        public ActionResult Delete(int id)
        { return View(); }

        // POST: Pharmacy/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
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
