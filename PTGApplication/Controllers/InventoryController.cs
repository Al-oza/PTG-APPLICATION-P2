﻿using PTGApplication.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace PTGApplication.Controllers
{
    public class InventoryController : Controller
    {
        // GET: Pharmacy
        public ActionResult Index()
        {
            TempData["UserRoleAdmin"] = User.IsInRole(Properties.UserRoles.PharmacyManager);
            TempData["UserRoleSiteManager"] = User.IsInRole(Properties.UserRoles.CareSiteInventoryManager);
            return View();
        }


        // GET: Pharmacy/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        public ActionResult SelectAddInventory()
        {
            using (var uzima = new UzimaRxEntities())
            {
                return View(uzima.UzimaDrugs.ToList());
            }
        }



        // GET: Pharmacy/AddInventory
        public ActionResult AddInventory(int id)
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

                var suppliers =
                    (from location in uzima.UzimaLocations
                     join type in uzima.UzimaLocationTypes on location.Id equals type.LocationId
                     where type.Supplier == null
                     select location).ToList();

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
                        Id = uzima.UzimaInventories.Count(),
                        StatusId = model.StatusId,
                        LastModifiedBy = user.Id
                    };


                    for (int i = 0; i < Convert.ToInt32(txtQuantity); i++)
                    {
                        uzima.UzimaInventories.Add(inventory);
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
                var currentInventory = (from inventory in uzima.UzimaInventories
                                        where inventory.Id == id
                                        select inventory).SingleOrDefault();

                var drugs = uzima.UzimaDrugs.ToList();
                if (!(drugs is null))
                {
                    var currentDrug = drugs.Where(drug => drug.Id == id);
                    ViewBag.Drugs = new SelectList(drugs, "Id", "DrugName", currentDrug);
                }

                var users = uzima.AspNetUsers.ToList();
                if (!(users is null))
                {
                    var currentUser = users.Where(user => user.Username == currentInventory.LastModifiedBy).SingleOrDefault();
                    ViewBag.Users = new SelectList(users, "Id", "Username", currentUser);
                }

                var statuses = uzima.UzimaStatus.ToList();
                if (!(statuses is null))
                {
                    var currentStatus = (from status in uzima.UzimaStatus
                                         where status.Id == currentInventory.StatusId
                                         select status.Status).SingleOrDefault();
                    ViewBag.Statuses = new SelectList(statuses, "Id", "Status", currentStatus);
                }

                var locations = uzima.UzimaLocations.ToList();
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

        // POST: Pharmacy/ModifyInventory
        [HttpPost]
        public async Task<ActionResult> ModifyInventory(UzimaInventory model)
        {
            using (var uzima = new UzimaRxEntities())
            {
                try
                {
                    uzima.UzimaInventories.Remove(
                        (from inventory in uzima.UzimaInventories
                         where inventory.Id == model.Id
                         select inventory).SingleOrDefault());
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

                return RedirectToAction("Index");
            }
        }

        // GET: Pharmacy/RemoveInventory/5
        public ActionResult RemoveInventory(int id)
        {
            using (var uzima = new UzimaRxEntities())
            {
                return View(uzima.UzimaInventories.SingleOrDefault(inv => inv.Id == id));
            }
        }

        // POST: Pharmacy/RemoveInventory
        [HttpPost]
        public async Task<ActionResult> RemoveInventory(int id, UzimaInventory model)
        {
            using (var uzima = new UzimaRxEntities())
            {
                try
                {
                    var inventory = (from inv in uzima.UzimaInventories
                                     where inv.Id == id
                                     select inv).SingleOrDefault();
                    uzima.UzimaInventories.Remove(inventory);
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

        public ActionResult AddtoDrugList()
        {
            return View();
        }

        [HttpPost]
        public ActionResult AddtoDrugList(UzimaDrug model)
        {
            using (var cs = new UzimaRxEntities())
            {
                try
                {
                    cs.UzimaDrugs.Add(new UzimaDrug()
                    {
                        Id = cs.UzimaDrugs.Count() + 1,
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
                return View(uzima.UzimaDrugs.SingleOrDefault(inv => inv.Id == id));
            }
        }

        // POST: Pharmacy/RemoveDrug
        [HttpPost]
        public async Task<ActionResult> RemoveDrugFromList(int id, UzimaDrug model)
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

        // GET: DrugRemoved
        public ActionResult DrugRemoved()
        {
            return View();
        }
    }
}
