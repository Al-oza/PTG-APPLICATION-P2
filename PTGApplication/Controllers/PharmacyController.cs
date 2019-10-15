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
        public async Task<ActionResult> AddInventory(string txtQuantity, PharmacyInventory model)
        {
            using (var uzima = new UzimaRxEntities())
            {
                var user = uzima.AspNetUsers.SingleOrDefault(u => u.Username == User.Identity.Name);
                try
                {
                    var inventory = new PharmacyInventory()
                    {
                        BarcodeId = model.BarcodeId,
                        CurrentLocationId = model.CurrentLocationId,
                        DateOrdered = model.DateOrdered,
                        ExpirationDate = model.ExpirationDate,
                        FutureLocationId = model.FutureLocationId,
                        Id = uzima.PharmacyInventories.Count(),
                        StatusId = model.StatusId,
                        UserId = user.Id
                    };


                    for (int i = 0; i < Convert.ToInt32(txtQuantity); i++)
                    {
                        uzima.PharmacyInventories.Add(inventory);
                        inventory.Id++;
                        await uzima.SaveChangesAsync();
                    }
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

        // GET: Pharmacy/ModifyInventory
        public ActionResult ModifyInventory(int id)
        {
            using (var uzima = new UzimaRxEntities())
            {
                var drugs = uzima.PharmacyDrugs.ToList();
                if (drugs != null) { ViewBag.drugs = drugs; }
                var users = uzima.AspNetUsers.ToList();
                if (users != null) { ViewBag.users = users; }
                var statuses = uzima.PharmacyStatus.ToList();
                if (statuses != null) { ViewBag.statuses = statuses; }
                var locations = uzima.PharmacyLocations.ToList();
                if (locations != null) { ViewBag.locations = locations; }
                var orderDate = uzima.PharmacyInventories.Where(i => i.Id == id).SingleOrDefault().DateOrdered;
                if (orderDate != null) { ViewBag.orderDate = orderDate; }
                var expirationDate = uzima.PharmacyInventories.Where(i => i.Id == id).SingleOrDefault().ExpirationDate;
                if (expirationDate != null) { ViewBag.expirationDate = expirationDate; }
                return View(uzima.PharmacyInventories.Single(m => m.Id == id));
            }
        }

        // POST: Pharmacy/ModifyInventory
        [HttpPost]
        public async Task<ActionResult> ModifyInventory(PharmacyInventory model)
        {
            using (var uzima = new UzimaRxEntities())
            {
                try
                {
                    uzima.PharmacyInventories.Remove(
                        uzima.PharmacyInventories.Where(i => i.Id == model.Id).SingleOrDefault());
                    uzima.PharmacyInventories.Add(model);

                    await uzima.SaveChangesAsync();
                    ViewBag.successMessage = "Inventory Modified";
                }
                catch (Exception ex)
                {
                    if (ex.InnerException != null)
                    {
                        if (ex.InnerException.InnerException != null)
                        { ViewBag.errorMessage = ex.InnerException.InnerException.Message; }
                        else { ViewBag.errorMessage = ex.InnerException.Message; }
                    }
                    else { ViewBag.errorMessage = ex.Message; }
                    return View("Error");
                }
                return RedirectToAction("Index");
            }
        }

        // GET: Pharmacy/RemoveInventory/5
        public ActionResult RemoveInventory(int id)
        {
            using (var uzima = new UzimaRxEntities())
            { return View(uzima.PharmacyInventories.SingleOrDefault(inv => inv.Id == id)); }
        }

        // POST: Pharmacy/RemoveInventory
        [HttpPost]
        public async Task<ActionResult> RemoveInventory(int id, PharmacyInventory model)
        {
            using (var uzima = new UzimaRxEntities())
            {
                try
                {
                    var inventory = (from inv in uzima.PharmacyInventories
                                     where inv.Id == id
                                     select inv).SingleOrDefault();
                    uzima.PharmacyInventories.Remove(inventory);
                    await uzima.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    if (ex.InnerException != null)
                    { ViewBag.errorMessage = ex.InnerException.Message; }
                    else { ViewBag.errorMessage = ex.Message; }
                    return View("Error");
                }

                return View("Index");
            }
        }

        // GET: Pharmacy/Select
        public ActionResult SelectInventory()
        {
            using (var uzima = new UzimaRxEntities())
            {
                var locations = uzima.PharmacyLocations.ToList();
                if (locations != null) { ViewBag.locations = locations; }
                return View(uzima.PharmacyInventories.ToList());
            }
        }

        public ActionResult AddtoDrugList()
        {
            return View();
        }
        [HttpPost]
        public ActionResult AddtoDrugList(PharmacyDrug model)
        {
            using (var cs = new UzimaRxEntities())
            {
                try
                {
                    var drug = new PharmacyDrug()
                    { 
                        Id = cs.PharmacyDrugs.Count() + 1,
                        Barcode = model.Barcode,
                        Name = model.Name,
                        BrandName = model.BrandName,
                        ApplicationNumber = model.ApplicationNumber,
                        Manufacturer = model.Manufacturer,
                        ManufacturerLocation = model.ManufacturerLocation,
                        ApprovalNumber = model.ApprovalNumber,
                        Schedule = model.Schedule,
                        License = model.License,
                        Ingredients = model.Ingredients,
                        PackSize = model.PackSize
                    };
                    cs.PharmacyDrugs.Add(drug);
                    cs.SaveChanges();
                }
                catch (Exception ex)
                {
                    ViewBag.errorMessage = ex.Message;
                    return View("Error");
                }
                return RedirectToAction("DrugAdded");
            }
        }

        // GET: DrugAdded
        public ActionResult DrugAdded()
        {
            return View();
        }

        public ActionResult RemoveFromDrugList()
        {
            return View();
        }

        [HttpPost]
        public ActionResult RemoveFromDrugList(PharmacyDrug model)
        {

            using (var cs = new UzimaRxEntities())
            {
                try
                {
                    var drug = new PharmacyDrug()
                    {
                        Id = model.Id,
                        Barcode = model.Barcode,
                        Name = model.Name,
                        BrandName = model.BrandName,
                        ApplicationNumber = model.ApplicationNumber,
                        Manufacturer = model.Manufacturer,
                        ManufacturerLocation = model.ManufacturerLocation,
                        ApprovalNumber = model.ApprovalNumber,
                        Schedule = model.Schedule,
                        License = model.License,
                        Ingredients = model.Ingredients,
                        PackSize = model.PackSize
                    };
                    cs.PharmacyDrugs.Remove(drug);
                    cs.SaveChanges();
                }
                catch (Exception ex)
                {
                    ViewBag.errorMessage = ex.Message;
                    return View("Error");
                }
                return RedirectToAction("DrugRemoved");
            }
        }

        // GET: Select Drug
        public ActionResult SelectDrug(String SearchString)
        {
            using (var uzima = new UzimaRxEntities())
            {
                return View(uzima.PharmacyDrugs.Where(m => m.Name.Contains(SearchString) || SearchString == null).ToList());
            }
        }

        // GET: DrugRemoved
        public ActionResult DrugRemoved()
        {
            return View();
        }
    }
}
