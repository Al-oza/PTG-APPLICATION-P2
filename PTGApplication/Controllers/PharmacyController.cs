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
        {
            using (var uzima = new UzimaRxEntities())
            {
                var drugs = uzima.PharmacyDrugs.ToList();
                if (drugs != null) { ViewBag.drugs = drugs; }
                var locations = uzima.PharmacyLocations.ToList();
                if (locations != null) { ViewBag.locations = locations; }
                var statuses = uzima.PharmacyStatus.ToList();
                if (statuses != null) { ViewBag.statuses = statuses; }
                return View();
            }
        }

        // POST: Pharmacy/AddInventory
        [HttpPost]
        public async Task<ActionResult> AddInventory(PharmacyInventory model)
        {
            using (var uzima = new UzimaRxEntities())
            {
                var user = uzima.AspNetUsers.SingleOrDefault(u => u.Username == User.Identity.Name);
                try
                {
                    var inventory = new PharmacyInventory();
                    inventory.BarcodeId = model.BarcodeId;
                    inventory.CurrentLocationId = model.CurrentLocationId;
                    inventory.DateOrdered = model.DateOrdered;
                    inventory.ExpirationDate = model.ExpirationDate;
                    inventory.FutureLocationId = model.FutureLocationId;
                    inventory.Id = uzima.PharmacyInventories.Count();
                    inventory.StatusId = model.StatusId;
                    inventory.UserId = user.Id;


                    uzima.PharmacyInventories.Add(inventory);
                    await uzima.SaveChangesAsync();
                    ViewBag.successMessage = "Inventory Added";
                }
                catch (Exception ex)
                {
                    if (ex.InnerException != null)
                    { ViewBag.errorMessage = ex.InnerException.Message; }
                    else { ViewBag.errorMessage = ex.Message; }
                    return View("Error");
                }

                return RedirectToAction("Index");
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
