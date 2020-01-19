using PTGApplication.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace PTGApplication.Controllers
{
    /// <summary>
    /// Controller for Inventory Management
    /// </summary>
    public class InventoryController : Controller
    {
        /// <summary>
        /// Get list of Inventory Management Actions
        /// </summary>
        /// <returns>A list of inventory actions</returns>
        // GET: Pharmacy
        public ActionResult Index()
        {
            TempData["UserRoleAdmin"] = User.IsInRole(Properties.UserRoles.PharmacyManager);
            TempData["UserRoleSysAdmin"] = User.IsInRole(Properties.UserRoles.SystemAdmin);
            TempData["UserRoleSiteManager"] = User.IsInRole(Properties.UserRoles.CareSiteInventoryManager);
            return View();
        }
        /// <summary>
        /// Get Drug List
        /// </summary>
        /// <returns>A table of drugs available</returns>
        public ActionResult SelectAddInventory()
        {
            using (var uzima = new UzimaRxEntities())
            {
                System.Collections.Generic.List<UzimaDrug> drugs;
                drugs = uzima.UzimaDrugs.ToList();
                return View(drugs);
            }
        }
        /// <summary>
        /// Order a given item
        /// </summary>
        /// <param name="id">ID of inventory item to order</param>
        /// <returns>An order page for the inventory item</returns>
        // GET: Pharmacy/AddInventory
        public ActionResult AddInventory(Guid id)
        {
            using (var uzima = new UzimaRxEntities())
            {
                var drugs = (from drug in uzima.UzimaDrugs
                             where drug.Id == id
                             select drug).ToList();

                if (!(drugs is null))
                {
                    ViewBag.Drug = new SelectList(drugs, "Id", "DrugName");
                }

                System.Collections.Generic.List<UzimaLocation> suppliers;
                if (User.IsInRole(Properties.UserRoles.SystemAdmin))
                {
                    suppliers =
                        (from location in uzima.UzimaLocations
                         join type in uzima.UzimaLocationTypes on location.Id equals type.LocationId
                         where type.Supplier == null
                         select location).ToList();
                }
                else
                {
                    var hp = (from location in uzima.UzimaLocations
                              join user in uzima.AspNetUsers on location.LocationName equals user.HomePharmacy
                              where user.Username == User.Identity.Name
                              select location).Single();
                    suppliers = (from location in uzima.UzimaLocations
                                 join type in uzima.UzimaLocationTypes on location.Id equals type.LocationId
                                 where type.Supplier == null && type.LocationId == hp.Id
                                 select location).ToList();
                }

                if (!(suppliers is null))
                {
                    ViewBag.Suppliers = new SelectList(suppliers, "Id", "LocationName");
                }

                var statuses = uzima.UzimaStatus.ToList();
                if (!(statuses is null))
                {
                    ViewBag.Statuses = new SelectList(statuses, "Id", "Status");
                }

                return View();
            }
        }
        /// <summary>
        /// Order an inventory item
        /// </summary>
        /// <param name="txtQuantity">Quantity of Items to Order</param>
        /// <param name="model">Information carried over from previous page - handled internally</param>
        /// <returns>Item Order Confirmation</returns>
        // POST: Pharmacy/AddInventory
        [HttpPost]
        public async Task<ActionResult> AddInventory(string txtQuantity, UzimaInventory model)
        {
            using (var uzima = new UzimaRxEntities())
            {
                var user = uzima.AspNetUsers.
                    SingleOrDefault(u => u.Username == User.Identity.Name);

                try
                {
                    var inventory = new UzimaInventory()
                    {
                        DrugId = model.DrugId,
                        CurrentLocationId = model.CurrentLocationId,
                        DateOrdered = model.DateOrdered,
                        ExpirationDate = model.ExpirationDate,
                        FutureLocationId = null,
                        Id = Guid.NewGuid(),
                        StatusId = model.StatusId,
                        LastModifiedBy = user.Id
                    };


                    for (int i = 0; i < Convert.ToInt32(txtQuantity); i++)
                    {
                        uzima.UzimaInventories.Add(inventory);
                        inventory.Id = Guid.NewGuid();
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

                return RedirectToAction("SelectAddInventory", "Inventory");
            }
        }
        /// <summary>
        /// Modify Information about an Inventory Item
        /// </summary>
        /// <param name="id">ID of Inventory Item to Modify</param>
        /// <returns>Item Modified Confirmation</returns>
        // GET: Pharmacy/ModifyInventory
        public ActionResult ModifyInventory(Guid id)
        {
            using (var uzima = new UzimaRxEntities())
            {
                var currentInventory = (from inventory in uzima.UzimaInventories where inventory.Id == id select inventory).Single();
                var currentDrug = (from drug in uzima.UzimaDrugs where drug.Id == currentInventory.DrugId select drug).Single();

                ViewBag.Drugs = new SelectList(new[] { currentDrug }, "Id", "DrugName");

                var statuses = uzima.UzimaStatus.ToList();
                if (!(statuses is null))
                {
                    ViewBag.Statuses = new SelectList(statuses, "Id", "Status", currentInventory.StatusId);
                }

                System.Collections.Generic.List<UzimaLocation> locations;
                if (User.IsInRole(Properties.UserRoles.SystemAdmin))
                {
                    locations =
                        (from location in uzima.UzimaLocations
                         join type in uzima.UzimaLocationTypes on location.Id equals type.LocationId
                         select location).ToList();
                }
                else
                {
                    var hp = (from location in uzima.UzimaLocations
                              join user in uzima.AspNetUsers on location.LocationName equals user.HomePharmacy
                              where user.Username == User.Identity.Name
                              select location).Single();
                    locations = (from location in uzima.UzimaLocations
                                 join type in uzima.UzimaLocationTypes on location.Id equals type.LocationId
                                 where type.LocationId == hp.Id || type.Supplier == hp.Id
                                 select location).ToList();
                }
                if (!(locations is null))
                {
                    ViewBag.Locations = locations;
                }

                DateTime? orderDate = (from inventory in uzima.UzimaInventories
                                       where inventory.Id == id
                                       select inventory.DateOrdered).SingleOrDefault();
                if (!(orderDate is null))
                {
                    ViewBag.orderDate = orderDate;
                }

                DateTime? expirationDate = (from inventory in uzima.UzimaInventories
                                            where inventory.Id == id
                                            select inventory.ExpirationDate).SingleOrDefault();
                if (!(expirationDate is null))
                {
                    ViewBag.expirationDate = expirationDate;
                }

                return View(uzima.UzimaInventories.SingleOrDefault(model => model.Id == id));
            }
        }
        /// <summary>
        /// Modify Inventory Item on Database
        /// </summary>
        /// <param name="model">Information carried over from previous page - handled internally</param>
        /// <returns>Item Modified in Database</returns>
        // POST: Pharmacy/ModifyInventory
        [HttpPost]
        public async Task<ActionResult> ModifyInventory(UzimaInventory model)
        {
            using (var uzima = new UzimaRxEntities())
            {
                try
                {
                    var userid = (from user in uzima.AspNetUsers
                                  where user.Username == User.Identity.Name
                                  select user.Id).Single();
                    uzima.UzimaInventories.Remove(
                        (from inventory in uzima.UzimaInventories
                         where inventory.Id == model.Id
                         select inventory).SingleOrDefault());
                    model.LastModifiedBy = userid;
                    uzima.UzimaInventories.Add(model);

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

                return RedirectToAction("SelectDrug");
            }
        }
        /// <summary>
        /// Remove an Item from the Database
        /// </summary>
        /// <param name="id">ID of Inventory Item to Remove</param>
        /// <returns>Confirmation Page</returns>
        // GET: Pharmacy/RemoveInventory/5
        public ActionResult RemoveInventory(Guid id)
        {
            using (var uzima = new UzimaRxEntities())
            {
                return View(uzima.UzimaInventories.SingleOrDefault(inv => inv.Id == id));
            }
        }
        /// <summary>
        /// Remove Inventory Item from Database
        /// </summary>
        /// <param name="id">ID of Inventory Item to Remove</param>
        /// <param name="model">Information carried over from previous page - handled internally</param>
        /// <returns>Inventory Item Removed from Database</returns>
        // POST: Pharmacy/RemoveInventory
        [HttpPost]
        public async Task<ActionResult> RemoveInventory(Guid id, UzimaInventory model)
        {
            using (var uzima = new UzimaRxEntities())
            {
                try
                {
                    var entity = uzima.UzimaInventories.Where(e => e.Id == id).SingleOrDefault();
                    uzima.UzimaInventories.Remove(entity);
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

                return RedirectToAction("SelectInventory");
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        // GET: Pharmacy/Select
        public ActionResult SelectInventory()
        {
            using (var uzima = new UzimaRxEntities())
            {
                var inventories = uzima.UzimaInventories.ToList();
                var locations = uzima.UzimaLocations.ToList();
                var drugs = uzima.UzimaDrugs.ToList();

                var drugNames = new String[inventories.Count];
                var locationNames = new string[inventories.Count];
                for (int i = 0; i < inventories.Count; i++)
                {
                    locationNames[i] = (from location in locations
                                        where location.Id == inventories[i].CurrentLocationId
                                        select location.LocationName).Single();
                    //locations.Where(loc => loc.Id == inventories[i].CurrentLocationId).Select(loc=>loc.LocationName).Single();
                    drugNames[i] = (from drug in drugs
                                    where drug.Id == inventories[i].DrugId
                                    select drug.DrugName).Single();
                }
                if (!(drugs is null))
                {
                    ViewBag.Drugs = drugNames;
                }

                if (!(locations is null))
                {
                    ViewBag.Locations = locationNames;
                }

                return View(inventories);
            }
        }
        /// <summary>
        /// Add a New Inventory Item to the Drug List
        /// </summary>
        /// <returns>Drug Information Page</returns>
        public ActionResult AddtoDrugList()
        {
            return View();
        }
        /// <summary>
        /// Add New Inventory Item to Database
        /// </summary>
        /// <param name="model">Information carried over from previous page - handled internally</param>
        /// <returns>Confirmation Page</returns>
        [HttpPost]
        public ActionResult AddtoDrugList(UzimaDrug model)
        {
            using (var cs = new UzimaRxEntities())
            {
                try
                {
                    cs.UzimaDrugs.Add(new UzimaDrug()
                    {
                        Id = Guid.NewGuid(),
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
        /// <summary>
        /// Drug Added Confirmation
        /// </summary>
        /// <returns>Confirmation Page</returns>
        // GET: DrugAdded
        public ActionResult DrugAdded()
        {
            return View();
        }
        /// <summary>
        /// Remove an Inventory Item from the Drug List
        /// </summary>
        /// <param name="id">ID of Inventory Item to Remove</param>
        /// <returns>Confirm Remove Item Page</returns>
        // GET: Pharmacy/RemoveDrugFromDrugList/5
        public ActionResult RemoveDrugFromList(Guid id)
        {
            using (var uzima = new UzimaRxEntities())
            {
                return View(uzima.UzimaDrugs.SingleOrDefault(inv => inv.Id == id));
            }
        }
        /// <summary>
        /// Remove Drug List Item from Database
        /// </summary>
        /// <param name="id">ID of Drug List Item to Remove</param>
        /// <param name="model">Information carried over from previous page - handled internally</param>
        /// <returns>Item Removed Confirmation</returns>
        // POST: Pharmacy/RemoveDrug
        [HttpPost]
        public async Task<ActionResult> RemoveDrugFromList(Guid id, UzimaDrug model)
        {
            using (var uzima = new UzimaRxEntities())
            {
                try
                {
                    var entity = uzima.UzimaDrugs.Where(e => e.Id == id).SingleOrDefault();
                    uzima.UzimaDrugs.Remove(entity);
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


                return RedirectToAction("SelectDrug");
            }
        }
        /// <summary>
        /// Modify Drug List Item Details
        /// </summary>
        /// <param name="id">ID of the Drug List Item to Modify</param>
        /// <returns>Modification Page</returns>
        // GET: Pharmacy/ModifyDrugFromDrugList/5
        public ActionResult ModifyDrugFromList(Guid id)
        {
            using (var uzima = new UzimaRxEntities())
            {
                return View(uzima.UzimaDrugs.SingleOrDefault(inv => inv.Id == id));
            }
        }
        /// <summary>
        /// Modify Drug List Item Details in Database
        /// </summary>
        /// <param name="id">ID of Drug List Item to Modify</param>
        /// <param name="model">Information carried over from previous page - handled internally</param>
        /// <returns>Drug List Item Modified Confirmation</returns>
        // POST: Pharmacy/ModifyDrug
        [HttpPost]
        public async Task<ActionResult> ModifyDrugFromList(Guid id, UzimaDrug model)
        {
            using (var uzima = new UzimaRxEntities())
            {
                try
                {
                    var drug = (from dr in uzima.UzimaDrugs
                                where dr.Id == id
                                select dr).SingleOrDefault();
                    uzima.UzimaDrugs.Remove(drug);
                    await uzima.SaveChangesAsync();

                    model.Id = id;
                    uzima.UzimaDrugs.Add(model);

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

                return RedirectToAction("Index", "Inventory");
            }
        }
        /// <summary>
        /// Search for Item in Drug List
        /// </summary>
        /// <param name="searchString">Search Input from User</param>
        /// <returns>Matched Items</returns>
        // GET: Select Drug
        public ActionResult SelectDrug(String searchString)
        {
            using (var uzima = new UzimaRxEntities())
            {
                var model = new List<UzimaDrug>();

                if (searchString is null)
                {
                    model = uzima.UzimaDrugs.ToList();
                }
                else
                {
                    model = (from drug in uzima.UzimaDrugs
                             where drug.DrugName.Contains(searchString)
                             select drug).ToList();
                }

                return View(model);
            }
        }
        /// <summary>
        /// Drug List Item Removed Confirmation
        /// </summary>
        /// <returns>Drug List Item Removed Confirmation Page</returns>
        // GET: DrugRemoved
        public ActionResult DrugRemoved()
        {
            return View();
        }
    }
}
