using PTGApplication.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace PTGApplication.Controllers
{
    public class PharmacyController : Controller
    {
        // GET: Pharmacy
        public ActionResult Index()
        {
            return View();
        }

        // GET: Pharmacy/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: Pharmacy/AddInventory
        public ActionResult AddInventory()
        {
            using (var uzima = new UzimaRxEntities())
            {
                var drugs = uzima.PharmacyDrugs.ToList();
                if (!(drugs is null))
                {
                    ViewBag.drugs = drugs;
                }

                var suppliers =
                    (from location in uzima.PharmacyLocations
                     join type in uzima.PharmacyLocationTypes on location.Id equals type.LocationId
                     where type.Supplier == null
                     select location).ToList();

                if (!(suppliers is null))
                {
                    ViewBag.Suppliers = new SelectList(suppliers, "Id", "DrugName");
                }

                var statuses = uzima.PharmacyStatus.ToList();
                if (!(statuses is null))
                {
                    ViewBag.Statuses = new SelectList(statuses, "Id", "Status");
                }

                return View();
            }
        }

        // POST: Pharmacy/AddInventory
        [HttpPost]
        public async Task<ActionResult> AddInventory(string txtQuantity, PharmacyInventory model)
        {
            using (var uzima = new UzimaRxEntities())
            {
                var user = uzima.AspNetUsers.
                    SingleOrDefault(u => u.Username == User.Identity.Name);

                try
                {
                    var inventory = new PharmacyInventory()
                    {
                        DrugId = model.DrugId,
                        CurrentLocationId = model.CurrentLocationId,
                        DateOrdered = model.DateOrdered,
                        ExpirationDate = model.ExpirationDate,
                        FutureLocationId = null,
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
                    {
                        ViewBag.errorMessage = ex.InnerException.Message;
                    }
                    else
                    {
                        ViewBag.errorMessage = ex.Message;
                    }

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
                if (!(drugs is null))
                {
                    ViewBag.drugs = drugs;
                }

                var users = uzima.AspNetUsers.ToList();
                if (!(users is null))
                {
                    ViewBag.users = users;
                }

                var statuses = uzima.PharmacyStatus.ToList();
                if (!(statuses is null))
                {
                    ViewBag.statuses = statuses;
                }

                var locations = uzima.PharmacyLocations.ToList();
                if (!(locations is null))
                {
                    ViewBag.locations = locations;
                }

                DateTime? orderDate = (from inventory in uzima.PharmacyInventories
                                 where inventory.Id == id
                                 select inventory.DateOrdered).SingleOrDefault();
                if (!(orderDate is null))
                {
                    ViewBag.orderDate = orderDate;
                }

                DateTime? expirationDate = (from inventory in uzima.PharmacyInventories
                                      where inventory.Id == id
                                      select inventory.ExpirationDate).SingleOrDefault();
                if (!(expirationDate is null))
                {
                    ViewBag.expirationDate = expirationDate;
                }

                return View(uzima.PharmacyInventories.SingleOrDefault(model => model.Id == id));
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
                        (from inventory in uzima.PharmacyInventories
                         where inventory.Id == model.Id
                         select inventory).SingleOrDefault());
                    uzima.PharmacyInventories.Add(model);

                    await uzima.SaveChangesAsync();
                    ViewBag.successMessage = "Inventory Modified";
                }
                catch (Exception ex)
                {
                    if (ex.InnerException is null)
                    {
                        ViewBag.errorMessage = ex.Message;
                    }
                    else
                    {
                        ViewBag.errorMessage = "Something went wrong internally";
                    }

                    return View("Error");
                }

                return RedirectToAction("Index");
            }
        }

        // GET: Pharmacy/RemoveInventory/5
        public ActionResult RemoveInventory(int id)
        {
            using (var uzima = new UzimaRxEntities())
            {
                return View(uzima.PharmacyInventories.SingleOrDefault(inv => inv.Id == id));
            }
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
                    if (ex.InnerException is null)
                    {
                        ViewBag.errorMessage = ex.Message;
                    }
                    else
                    {
                        ViewBag.errorMessage = "Something went wrong internally";
                    }

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

                if (!(locations is null))
                {
                    ViewBag.locations = locations;
                }

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
                    cs.PharmacyDrugs.Add(new PharmacyDrug()
                    {
                        Id = cs.PharmacyDrugs.Count() + 1,
                        Barcode = model.Barcode,
                        DrugName = model.DrugName,
                        BrandName = model.BrandName,
                        ApplicationNumber = model.ApplicationNumber,
                        Manufacturer = model.Manufacturer,
                        ManufacturerLocation = model.ManufacturerLocation,
                        ApprovalNumber = model.ApprovalNumber,
                        Schedule = model.Schedule,
                        License = model.License,
                        Ingredients = model.Ingredients,
                        PackSize = model.PackSize
                    });

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

        // GET: Pharmacy/RemoveDrugFromDrugList/5
        public ActionResult RemoveDrugFromList(int id)
        {
            using (var uzima = new UzimaRxEntities())
            {
                return View(uzima.PharmacyDrugs.SingleOrDefault(inv => inv.Id == id));
            }
        }

        // POST: Pharmacy/RemoveDrug
        [HttpPost]
        public async Task<ActionResult> RemoveDrugFromList(int id, PharmacyDrug model)
        {
            using (var uzima = new UzimaRxEntities())
            {
                try
                {
                    var drug = (from dr in uzima.PharmacyDrugs
                                where dr.Id == id
                                select dr).SingleOrDefault();
                    uzima.PharmacyDrugs.Remove(drug);
                    await uzima.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    if (ex.InnerException is null)
                    {
                        ViewBag.errorMessage = ex.Message;
                    }
                    else
                    {
                        ViewBag.errorMessage = "Something went wrong internally";
                    }

                    return View("Error");
                }

                return View("Index");
            }
        }


        // GET: Select Drug
        public ActionResult SelectDrug(String searchString)
        {
            using (var uzima = new UzimaRxEntities())
            {
                var model = new List<PharmacyDrug>();

                if (searchString is null)
                {
                    model = uzima.PharmacyDrugs.ToList();
                }
                else
                {
                    model = (from drug in uzima.PharmacyDrugs
                             where drug.DrugName.Contains(searchString)
                             select drug).ToList();
                }

                return View(model);
            }
        }

        // GET: DrugRemoved
        public ActionResult DrugRemoved()
        {
            return View();
        }
    }
}
