using PTGApplication.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace PTGApplication.Controllers
{
    public class PharmacyController : Controller
    {
        // GET: Pharmacy
        public ActionResult Index()
        { return View(); }

        // GET: Pharmacy/Details/5
        public ActionResult Details(int id)
        { return View(); }

        // GET: Pharmacy/AddInventory
        public ActionResult AddInventory()
        { return View(); }

        // POST: Pharmacy/AddInventory
        [HttpPost]
        public async Task<ActionResult> AddInventory(PharmacyDrugBrand model)
        {
            try
            {
                var uzima = new UzimaRxEntities();
                uzima.PharmacyDrugBrands.Add(model);
                await uzima.SaveChangesAsync();
                ViewBag.successMessage = "Inventory Added";
                uzima.Dispose();
            }
            catch(Exception ex)
            {
                ViewBag.errorMessage = ex.Message;
                return View("Error");
            }

            return View("AddInventory");
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
