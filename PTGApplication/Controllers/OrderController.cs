using Microsoft.AspNet.Identity;
using PTGApplication.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace PTGApplication.Controllers
{
    public class OrderController : Controller
    {
        // GET: Order
        public ActionResult Index()
        {
            return View();
        }

        // GET: Order/PlaceOrder
        public ActionResult PlaceOrder()
        {
            IEnumerable<PharmacyDrug> drugs;
            IEnumerable<PharmacyLocation> locations;
            IEnumerable<PharmacyInventory> inventories;
            IEnumerable<PharmacyLocationType> locationTypes;
            IEnumerable<AspNetUser> users;
            using (var uzima = new UzimaRxEntities())
            {
                drugs = uzima.PharmacyDrugs.ToList();
                locations = uzima.PharmacyLocations.ToList();
                inventories = uzima.PharmacyInventories.ToList();
                locationTypes = uzima.PharmacyLocationTypes.ToList();
                users = uzima.AspNetUsers.ToList();
            }

            if (!(drugs is null) && !(inventories is null))
            {

                var inventorydrugs =
                    (from drug in drugs
                     join inventory in inventories on drug.Id equals inventory.DrugId
                     where drug.Id == inventory.DrugId && inventory.StatusId == 0 && inventory.FutureLocationId == null
                     select drug
                     );

                ViewBag.Drugs = new SelectList(inventorydrugs.Distinct(), "Id", "DrugName");

            }

            

            if (!(locations is null) && !(locationTypes is null))
            {
                var userhomelocation =
                   (from location in locations
                    join user in users on location.LocationName equals user.HomePharmacy
                    where user.Username == User.Identity.Name
                    select location);

                ViewBag.LocationNeeded = new SelectList(userhomelocation, "Id", "DrugName");
            }
            return View();
        }
        // POST: Order/PlaceOrder
        [HttpPost]
        public async Task<ActionResult> PlaceOrder(String txtQty, PharmacyInventory model)
        {

            int id;
            string userid;

            try
            {
                using (var uzima = new UzimaRxEntities())
                {
                    userid =
                        (from user in uzima.AspNetUsers
                         where user.Username == User.Identity.Name
                         select user.Id).SingleOrDefault();
                    for (int i = 0; i < Convert.ToInt32(txtQty); i++)
                    {

                    id =
                        (from drug in uzima.PharmacyInventories
                         join location in uzima.PharmacyLocationTypes on drug.CurrentLocationId equals location.LocationId
                         where location.Supplier == null && drug.StatusId == 0 && model.DrugId == drug.DrugId
                         select drug.Id).FirstOrDefault();


                    var entryToEdit = uzima.PharmacyInventories.Find(id);
                    uzima.PharmacyInventories.Remove(entryToEdit);
                    await uzima.SaveChangesAsync();

                    entryToEdit.FutureLocationId = model.FutureLocationId;
                    entryToEdit.DateOrdered = DateTime.Now;
                    entryToEdit.UserId = userid;
                    entryToEdit.StatusId = 1;


                        uzima.PharmacyInventories.Add(entryToEdit);
                        await uzima.SaveChangesAsync();
                    }                   
                }
            }
            catch (Exception ex)
            {
                if (ex.InnerException is null)
                {
                    ViewBag.errorMessage = ex.Message;
                }
                else
                {
                    ViewBag.errorMessage = "Something went wrong internally.";
                }

                return View("Error");
            }

            return RedirectToAction("PlaceOrder");
        }



        // GET: Order/SendOrder
        public ActionResult SendOrder()
        {
            IEnumerable<PharmacyDrug> drugs;
            IEnumerable<PharmacyLocation> locations;
            IEnumerable<PharmacyInventory> inventories;
            IEnumerable<PharmacyLocationType> locationTypes;
            using (var uzima = new UzimaRxEntities())
            {
                drugs = uzima.PharmacyDrugs.ToList();
                locations = uzima.PharmacyLocations.ToList();
                inventories = uzima.PharmacyInventories.ToList();
                locationTypes = uzima.PharmacyLocationTypes.ToList();
            }

            if (!(drugs is null) && !(inventories is null))
            {
                var inventorydrugs =
                    (from drug in drugs
                     join inventory in inventories on drug.Id equals inventory.DrugId
                     where drug.Id == inventory.DrugId && inventory.StatusId == 1
                     select drug
                     );

                ViewBag.Drugs = new SelectList(inventorydrugs.Distinct(), "Id", "DrugName");

                if (inventorydrugs.Count() == 0)
                {

                    ViewBag.errorMessage = "There are currently no orders to be sent.";
                    return View("Error");
                }
            }

            if (!(locations is null) && !(locationTypes is null))
            {
                var clinics =
                    (from location in locations
                     join type in locationTypes on location.Id equals type.LocationId
                     where type.Supplier != null
                     select location);

                ViewBag.LocationNeeded = new SelectList(clinics, "Id", "DrugName");
            }
            return View();
        }


        // POST: Order/SendOrder
        [HttpPost]
        public async Task<ActionResult> SendOrder(String txtQty, PharmacyInventory model)
        {

            int id;
            string userid;

            try
            {
                using (var uzima = new UzimaRxEntities())
                {
                    userid =
                        (from user in uzima.AspNetUsers
                         where user.Username == User.Identity.Name
                         select user.Id).SingleOrDefault();
                    for (int i = 0; i < Convert.ToInt32(txtQty); i++)
                    {

                        id =
                            (from drug in uzima.PharmacyInventories
                             join location in uzima.PharmacyLocationTypes on drug.CurrentLocationId equals location.LocationId
                             where location.Supplier == null && drug.StatusId == 1 && model.DrugId == drug.DrugId
                             select drug.Id).FirstOrDefault();


                        var entryToEdit = uzima.PharmacyInventories.Find(id);
                        uzima.PharmacyInventories.Remove(entryToEdit);
                        await uzima.SaveChangesAsync();

                        entryToEdit.FutureLocationId = model.FutureLocationId;
                        entryToEdit.DateOrdered = DateTime.Now;
                        entryToEdit.UserId = userid;
                        entryToEdit.StatusId = 2;


                        uzima.PharmacyInventories.Add(entryToEdit);
                        await uzima.SaveChangesAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex.InnerException is null)
                {
                    ViewBag.errorMessage = ex.Message;
                }
                else
                {
                    ViewBag.errorMessage = "Something went wrong internally.";
                }

                return View("Error");
            }

            return RedirectToAction("SendOrder");
        }

        // GET: Order/RecieveOrder
        public ActionResult RecieveOrder()
        {
            IEnumerable<PharmacyDrug> drugs;
            IEnumerable<PharmacyLocation> locations;
            IEnumerable<PharmacyInventory> inventories;
            IEnumerable<PharmacyLocationType> locationTypes;
            IEnumerable<AspNetUser> users;

            using (var uzima = new UzimaRxEntities())
            {
                drugs = uzima.PharmacyDrugs.ToList();
                locations = uzima.PharmacyLocations.ToList();
                inventories = uzima.PharmacyInventories.ToList();
                locationTypes = uzima.PharmacyLocationTypes.ToList();
                users = uzima.AspNetUsers.ToList();
            }

            if (!(drugs is null) && !(inventories is null))
            {

                var userhomelocation =
                    (from location in locations
                     join user in users on location.LocationName equals user.HomePharmacy
                     where user.Username == User.Identity.Name
                     select location.Id).SingleOrDefault();
                    
                var inventorydrugs =
                    (from drug in drugs
                     join inventory in inventories on drug.Id equals inventory.DrugId
                     join location in locations on inventory.FutureLocationId equals location.Id
                     join user in users on location.LocationName equals user.HomePharmacy
                     where drug.Id == inventory.DrugId && inventory.StatusId == 2 && inventory.FutureLocationId == userhomelocation
                     select drug
                     );

                ViewBag.Drugs = new SelectList(inventorydrugs.Distinct(), "Id", "DrugName");

                if (inventorydrugs.Count() == 0) { 

                    ViewBag.errorMessage = "You have no drugs on order to be recieved.";
                    return View("Error");
                }
            }

            if (!(locations is null) && !(locationTypes is null))
            {
                var userhomelocation =
                    (from location in locations
                     join user in users on location.LocationName equals user.HomePharmacy
                     where user.Username == User.Identity.Name
                     select location);

                ViewBag.LocationRecieved = new SelectList(userhomelocation, "Id", "DrugName");
            }
            return View();
        }


        // POST: Order/RecieveOrder
        [HttpPost]
        public async Task<ActionResult> RecieveOrder(String txtQty, PharmacyInventory model)
        {

            int id;
            string userid;

            try
            {
                using (var uzima = new UzimaRxEntities())
                {

                    var userhomelocation =
                    (from location in uzima.PharmacyLocations
                     join user in uzima.AspNetUsers on location.LocationName equals user.HomePharmacy
                     where user.Username == User.Identity.Name
                     select location.Id).SingleOrDefault();

                    userid =
                        (from user in uzima.AspNetUsers
                         where user.Username == User.Identity.Name
                         select user.Id).SingleOrDefault();
                    for (int i = 0; i < Convert.ToInt32(txtQty); i++)
                    {

                        id =
                            (from drug in uzima.PharmacyInventories
                             join location in uzima.PharmacyLocationTypes on drug.CurrentLocationId equals location.LocationId
                             where drug.StatusId == 2 && model.DrugId == drug.DrugId && drug.FutureLocationId == userhomelocation
                             select drug.Id).FirstOrDefault();


                        var entryToEdit = uzima.PharmacyInventories.Find(id);
                        uzima.PharmacyInventories.Remove(entryToEdit);
                        await uzima.SaveChangesAsync();

                        entryToEdit.FutureLocationId = model.FutureLocationId;
                        entryToEdit.CurrentLocationId = (int)model.FutureLocationId;
                        entryToEdit.DateOrdered = DateTime.Now;
                        entryToEdit.UserId = userid;
                        entryToEdit.StatusId = 0;


                        uzima.PharmacyInventories.Add(entryToEdit);
                        await uzima.SaveChangesAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex.InnerException is null)
                {
                    ViewBag.errorMessage = ex.Message;
                }
                else
                {
                    ViewBag.errorMessage = "Something went wrong internally.";
                }

                return View("Error");
            }

            return RedirectToAction("RecieveOrder");
        }

        // GET: Order/DispenseDrug
        public ActionResult DispenseDrug()
        {
            IEnumerable<PharmacyDrug> drugs;
            IEnumerable<PharmacyLocation> locations;
            IEnumerable<PharmacyInventory> inventories;
            IEnumerable<PharmacyLocationType> locationTypes;
            IEnumerable<AspNetUser> users;

            using (var uzima = new UzimaRxEntities())
            {
                drugs = uzima.PharmacyDrugs.ToList();
                locations = uzima.PharmacyLocations.ToList();
                inventories = uzima.PharmacyInventories.ToList();
                locationTypes = uzima.PharmacyLocationTypes.ToList();
                users = uzima.AspNetUsers.ToList();
            }

            if (!(drugs is null) && !(inventories is null))
            {

                var userhomelocation =
                    (from location in locations
                     join user in users on location.LocationName equals user.HomePharmacy
                     where user.Username == User.Identity.Name
                     select location.Id).SingleOrDefault();

                var inventorydrugs =
                    (from drug in drugs
                     join inventory in inventories on drug.Id equals inventory.DrugId
                     join location in locations on inventory.FutureLocationId equals location.Id
                     join user in users on location.LocationName equals user.HomePharmacy
                     where drug.Id == inventory.DrugId && inventory.StatusId == 0 && inventory.FutureLocationId == userhomelocation
                     select drug
                     );

                ViewBag.Drugs = new SelectList(inventorydrugs.Distinct(), "Id", "DrugName");

                if (inventorydrugs.Count() == 0)
                {

                    ViewBag.errorMessage = "You have no drugs in inventory to dispense. (Please input drug as recieved before dispensing.)";
                    return View("Error");
                }
            }

            if (!(locations is null) && !(locationTypes is null))
            {
                var userhomelocation =
                    (from location in locations
                     join user in users on location.LocationName equals user.HomePharmacy
                     where user.Username == User.Identity.Name
                     select location);

                ViewBag.LocationDispensed = new SelectList(userhomelocation, "Id", "DrugName");
            }
            return View();
        }


        // POST: Order/DispenseDrug
        [HttpPost]
        public async Task<ActionResult> DispenseDrug(String txtQty, PharmacyInventory model)
        {

            int id;
            string userid;

            try
            {
                using (var uzima = new UzimaRxEntities())
                {

                    var userhomelocation =
                    (from location in uzima.PharmacyLocations
                     join user in uzima.AspNetUsers on location.LocationName equals user.HomePharmacy
                     where user.Username == User.Identity.Name
                     select location.Id).SingleOrDefault();

                    userid =
                        (from user in uzima.AspNetUsers
                         where user.Username == User.Identity.Name
                         select user.Id).SingleOrDefault();
                    for (int i = 0; i < Convert.ToInt32(txtQty); i++)
                    {

                        id =
                            (from drug in uzima.PharmacyInventories
                             where drug.StatusId == 0 && model.DrugId == drug.DrugId && drug.FutureLocationId == userhomelocation
                             select drug.Id).FirstOrDefault();


                        var entryToEdit = uzima.PharmacyInventories.Find(id);
                        uzima.PharmacyInventories.Remove(entryToEdit);
                        await uzima.SaveChangesAsync();

                        entryToEdit.FutureLocationId = model.FutureLocationId;
                        entryToEdit.CurrentLocationId = (int)model.FutureLocationId;
                        entryToEdit.DateOrdered = DateTime.Now;
                        entryToEdit.UserId = userid;
                        entryToEdit.StatusId = 3;


                        uzima.PharmacyInventories.Add(entryToEdit);
                        await uzima.SaveChangesAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex.InnerException is null)
                {
                    ViewBag.errorMessage = ex.Message;
                }
                else
                {
                    ViewBag.errorMessage = "Something went wrong internally.";
                }

                return View("Error");
            }

            return RedirectToAction("DispenseDrug");
        }


        // GET: Order/DestroyDrug
        public ActionResult DestroyDrug()
        {
            IEnumerable<PharmacyDrug> drugs;
            IEnumerable<PharmacyLocation> locations;
            IEnumerable<PharmacyInventory> inventories;
            IEnumerable<PharmacyLocationType> locationTypes;
            IEnumerable<AspNetUser> users;

            using (var uzima = new UzimaRxEntities())
            {
                drugs = uzima.PharmacyDrugs.ToList();
                locations = uzima.PharmacyLocations.ToList();
                inventories = uzima.PharmacyInventories.ToList();
                locationTypes = uzima.PharmacyLocationTypes.ToList();
                users = uzima.AspNetUsers.ToList();
            }

            if (!(drugs is null) && !(inventories is null))
            {

                var userhomelocation =
                    (from location in locations
                     join user in users on location.LocationName equals user.HomePharmacy
                     where user.Username == User.Identity.Name
                     select location.Id).SingleOrDefault();

                var inventorydrugs =
                    (from drug in drugs
                     join inventory in inventories on drug.Id equals inventory.DrugId
                     join location in locations on inventory.FutureLocationId equals location.Id
                     join user in users on location.LocationName equals user.HomePharmacy
                     where drug.Id == inventory.DrugId && inventory.StatusId == 0 && inventory.FutureLocationId == userhomelocation
                     select drug
                     );

                ViewBag.Drugs = new SelectList(inventorydrugs.Distinct(), "Id", "DrugName");

                if (inventorydrugs.Count() == 0)
                {

                    ViewBag.errorMessage = "You have no drugs in inventory to destroy. (If drugs were destroyed in transit, please input as recieved and then destroy.)";
                    return View("Error");
                }
            }

            if (!(locations is null) && !(locationTypes is null))
            {
                var userhomelocation =
                    (from location in locations
                     join user in users on location.LocationName equals user.HomePharmacy
                     where user.Username == User.Identity.Name
                     select location);

                ViewBag.LocationDestroyed = new SelectList(userhomelocation, "Id", "DrugName");
            }
            return View();
        }


        // POST: Order/DestroyOrder
        [HttpPost]
        public async Task<ActionResult> DestroyDrug(String txtQty, PharmacyInventory model)
        {

            int id;
            string userid;

            try
            {
                using (var uzima = new UzimaRxEntities())
                {

                    var userhomelocation =
                    (from location in uzima.PharmacyLocations
                     join user in uzima.AspNetUsers on location.LocationName equals user.HomePharmacy
                     where user.Username == User.Identity.Name
                     select location.Id).SingleOrDefault();

                    userid =
                        (from user in uzima.AspNetUsers
                         where user.Username == User.Identity.Name
                         select user.Id).SingleOrDefault();
                    for (int i = 0; i < Convert.ToInt32(txtQty); i++)
                    {

                        id =
                            (from drug in uzima.PharmacyInventories
                             where drug.StatusId == 0 && model.DrugId == drug.DrugId && drug.FutureLocationId == userhomelocation
                             select drug.Id).FirstOrDefault();


                        var entryToEdit = uzima.PharmacyInventories.Find(id);
                        uzima.PharmacyInventories.Remove(entryToEdit);
                        await uzima.SaveChangesAsync();

                        entryToEdit.FutureLocationId = model.FutureLocationId;
                        entryToEdit.CurrentLocationId = (int)model.FutureLocationId;
                        entryToEdit.DateOrdered = DateTime.Now;
                        entryToEdit.UserId = userid;
                        entryToEdit.StatusId = 4;


                        uzima.PharmacyInventories.Add(entryToEdit);
                        await uzima.SaveChangesAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex.InnerException is null)
                {
                    ViewBag.errorMessage = ex.Message;
                }
                else
                {
                    ViewBag.errorMessage = "Something went wrong internally.";
                }

                return View("Error");
            }

            return RedirectToAction("DestroyDrug");
        }

    }
}
