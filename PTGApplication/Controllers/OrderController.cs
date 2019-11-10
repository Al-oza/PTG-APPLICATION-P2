using Microsoft.AspNet.Identity;
using PTGApplication.Models;
using PTGApplication.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace PTGApplication.Controllers
{
    public class OrderController : Controller
    {
        // GET: Order
        public ActionResult Index()
        {
            TempData["UserRole"] = User.IsInRole(Properties.UserRoles.PharmacyManager);
            return View();
        }

        // GET: Order/PlaceOrder
        public ActionResult PlaceOrder(int id, int qty)
        {
            using (var uzima = new UzimaRxEntities())
            {
                ViewBag.Quantity = qty;
                var inventory = (from i in uzima.UzimaDrugs where i.Id == id select i).Single();
                ViewBag.Drug = inventory;

                var locations = uzima.UzimaLocations.ToList();
                var users = uzima.AspNetUsers.ToList();
                if (!(locations is null))
                {

                    var userhomelocation = (User.IsInRole(Properties.UserRoles.PharmacyManager)) ?
                        (from location in locations select location) :
                       (from location in locations
                        join user in users on location.LocationName equals user.HomePharmacy
                        where user.Username == User.Identity.Name
                        select location);

                    ViewBag.LocationNeeded = new SelectList(userhomelocation, "Id", "LocationName");
                }

                return View();
            }
        }
        // POST: Order/PlaceOrder
        [HttpPost]
        public async Task<ActionResult> PlaceOrder(String drugname, String txtQty, UzimaInventory model)
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
                            (from drug in uzima.UzimaInventories
                             join location in uzima.UzimaLocationTypes on drug.CurrentLocationId equals location.LocationId
                             where location.Supplier == null && drug.StatusId == 0 && drugname == drug.UzimaDrug.DrugName
                             select drug.Id).FirstOrDefault();


                        var entryToEdit = uzima.UzimaInventories.Find(id);
                        uzima.UzimaInventories.Remove(entryToEdit);
                        await uzima.SaveChangesAsync();

                        entryToEdit.FutureLocationId = model.FutureLocationId;
                        entryToEdit.DateOrdered = DateTime.Now;
                        entryToEdit.LastModifiedBy = userid;
                        entryToEdit.StatusId = 1;


                        uzima.UzimaInventories.Add(entryToEdit);
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

            return RedirectToAction("OrderPlaced");
        }

        // GET: Order/SelectPlaceOrder
        public ActionResult SelectPlaceOrder()
        {
            AspNetUser user;
            UzimaLocationType homePharmacy;
            UzimaLocation homeSupplier;
            using (var uzima = new UzimaRxEntities())
            {
                var userId = User.Identity.GetUserId();
                user = (from u in uzima.AspNetUsers where u.Id == userId select u).Single();
                homePharmacy = (from lt in uzima.UzimaLocationTypes join l in uzima.UzimaLocations on lt.LocationId equals l.Id where l.LocationName == user.HomePharmacy select lt).Single();
                homeSupplier = User.IsInRole(Properties.UserRoles.PharmacyManager) ?
                    (from l in uzima.UzimaLocations where l.Id == homePharmacy.Id select l).Single()
                   : (from l in uzima.UzimaLocations
                      join lt in uzima.UzimaLocationTypes on l.Id equals lt.LocationId
                      where lt.LocationId == homePharmacy.Supplier
                      select l).Single();

                var query = User.IsInRole(Properties.UserRoles.PharmacyManager) ?
                    "SELECT [UzimaDrug].Id, [UzimaDrug].DrugName AS 'Drug Name', COUNT(*) AS Quantity, [UzimaInventory].ExpirationDate AS 'Expiration Date' " +
                    "FROM UzimaInventory LEFT JOIN [UzimaDrug] ON [UzimaDrug].Id=[UzimaInventory].DrugId " +
                    "WHERE [UzimaInventory].StatusId=0 AND [UzimaInventory].FutureLocationId IS NULL " +
                    "GROUP BY [UzimaDrug].Id, [UzimaDrug].DrugName, [UzimaInventory].ExpirationDate" :
                    "SELECT [UzimaDrug].Id, [UzimaDrug].DrugName AS 'Drug Name', COUNT(*) AS Quantity, [UzimaInventory].ExpirationDate AS 'Expiration Date' " +
                    "FROM UzimaInventory LEFT JOIN [UzimaDrug] ON [UzimaDrug].Id=[UzimaInventory].DrugId " +
                    $"WHERE [UzimaInventory].StatusId=0 AND [UzimaInventory].CurrentLocationId={homeSupplier.Id} AND [UzimaInventory].FutureLocationId IS NULL " +
                    "GROUP BY [UzimaDrug].Id, [UzimaDrug].DrugName, [UzimaInventory].ExpirationDate";
                using (var dataSet = ConnectionPool.Query(query, "UzimaDrug", "UzimaInventory"))
                {
                    ViewBag.Columns = dataSet.Tables[0].Columns;
                    ViewBag.Data = dataSet.Tables[0].Rows;
                    return View();
                }
            }
        }

        //GET: Order/OrderPlaced
        public ActionResult OrderPlaced()
        {
            return View();
        }



        // GET: Order/SendOrder
        public ActionResult SendOrder(int id, int qty, string futureLocation)
        {
            if (!User.IsInRole(Properties.UserRoles.PharmacyManager))
            {
                Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            }

            using (var uzima = new UzimaRxEntities())
            {
                ViewBag.Quantity = qty;
                var location = (from l in uzima.UzimaLocations where l.LocationName == futureLocation select l).Single();
                var inventory = (from i in uzima.UzimaInventories where i.DrugId == id && i.FutureLocationId == location.Id select i).Single();
                ViewBag.Drug = inventory;

                var locations = uzima.UzimaLocations.ToList();
                var users = uzima.AspNetUsers.ToList();
                if (!(locations is null))
                {
                    ViewBag.LocationNeeded = location.LocationName;
                }

                return View();
            }
        }

        // POST: Order/SendOrder
        [HttpPost]
        public async Task<ActionResult> SendOrder(String drugname, String txtQty, string futureLocation, UzimaInventory model)
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
                            (from drug in uzima.UzimaInventories
                             join location in uzima.UzimaLocationTypes on drug.CurrentLocationId equals location.LocationId
                             where location.Supplier == null && drug.StatusId == 1 && drugname == drug.UzimaDrug.DrugName
                             select drug.Id).FirstOrDefault();


                        var entryToEdit = uzima.UzimaInventories.Find(id);
                        uzima.UzimaInventories.Remove(entryToEdit);
                        await uzima.SaveChangesAsync();

                        entryToEdit.FutureLocationId = model.FutureLocationId;
                        entryToEdit.DateOrdered = DateTime.Now;
                        entryToEdit.LastModifiedBy = userid;
                        entryToEdit.StatusId = 2;


                        uzima.UzimaInventories.Add(entryToEdit);
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

            return RedirectToAction("OrderSent");
        }
        // GET: Order/SelectSendOrder
        public ActionResult SelectSendOrder()
        {
            AspNetUser user;
            UzimaLocationType homePharmacy;
            using (var uzima = new UzimaRxEntities())
            {
                var userId = User.Identity.GetUserId();
                user = (from u in uzima.AspNetUsers where u.Id == userId select u).Single();
                homePharmacy = (from lt in uzima.UzimaLocationTypes join l in uzima.UzimaLocations on lt.LocationId equals l.Id where l.LocationName == user.HomePharmacy select lt).Single();
                var query =
                    "SELECT [UzimaDrug].Id, [UzimaDrug].DrugName AS 'Drug Name', COUNT(*) AS Quantity, " +
                    "[UzimaLocation].LocationName AS 'Send To', [UzimaInventory].ExpirationDate AS 'Expiration Date' FROM UzimaInventory " +
                    "LEFT JOIN [UzimaDrug] ON [UzimaDrug].Id=[UzimaInventory].DrugId " +
                    "LEFT JOIN [UzimaLocation] ON [UzimaInventory].FutureLocationId=[UzimaLocation].Id " +
                    $"WHERE [UzimaInventory].StatusId=1 AND [UzimaInventory].CurrentLocationId={homePharmacy.Id} " +
                    "GROUP BY [UzimaDrug].Id, [UzimaDrug].DrugName, [UzimaLocation].LocationName, [UzimaInventory].ExpirationDate";
                using (var dataSet = ConnectionPool.Query(query, "UzimaDrug", "UzimaInventory"))
                {
                    ViewBag.Columns = dataSet.Tables[0].Columns;
                    ViewBag.Data = dataSet.Tables[0].Rows;
                    return View();
                }
            }
        }

        // GET: OrderSent
        public ActionResult OrderSent()
        {
            return View();
        }

        // GET: Order/RecieveOrder
        public ActionResult RecieveOrder(int id, int qty)
        {
            using (var uzima = new UzimaRxEntities())
            {
                ViewBag.Quantity = qty;
                var inventory = (from i in uzima.UzimaDrugs where i.Id == id select i).Single();
                ViewBag.Drug = inventory;

                var locations = uzima.UzimaLocations.ToList();
                var users = uzima.AspNetUsers.ToList();
                if (!(locations is null))
                {

                    var userhomelocation = (User.IsInRole(Properties.UserRoles.PharmacyManager)) ?
                        (from location in locations select location) :
                       (from location in locations
                        join user in users on location.LocationName equals user.HomePharmacy
                        where user.Username == User.Identity.Name
                        select location);

                    ViewBag.LocationNeeded = new SelectList(userhomelocation, "Id", "LocationName");
                }

                return View();
            }
        }


        // POST: Order/RecieveOrder
        [HttpPost]
        public async Task<ActionResult> RecieveOrder(String drugname, String txtQty, UzimaInventory model)
        {

            int id;
            string userid;

            try
            {
                using (var uzima = new UzimaRxEntities())
                {

                    var userhomelocation =
                    (from location in uzima.UzimaLocations
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
                            (from drug in uzima.UzimaInventories
                             join location in uzima.UzimaLocationTypes on drug.CurrentLocationId equals location.LocationId
                             where drug.StatusId == 2 && drugname == drug.UzimaDrug.DrugName && drug.FutureLocationId == userhomelocation
                             select drug.Id).FirstOrDefault();


                        var entryToEdit = uzima.UzimaInventories.Find(id);
                        uzima.UzimaInventories.Remove(entryToEdit);
                        await uzima.SaveChangesAsync();

                        entryToEdit.FutureLocationId = model.FutureLocationId;
                        entryToEdit.CurrentLocationId = (int)model.FutureLocationId;
                        entryToEdit.DateOrdered = DateTime.Now;
                        entryToEdit.LastModifiedBy = userid;
                        entryToEdit.StatusId = 0;


                        uzima.UzimaInventories.Add(entryToEdit);
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

        // GET: Order/SelectRecieveOrder
        public ActionResult SelectRecieveOrder()
        {
            AspNetUser user;
            UzimaLocationType homePharmacy;
            using (var uzima = new UzimaRxEntities())
            {
                var userId = User.Identity.GetUserId();
                user = (from u in uzima.AspNetUsers where u.Id == userId select u).Single();
                homePharmacy = (from lt in uzima.UzimaLocationTypes join l in uzima.UzimaLocations on lt.LocationId equals l.Id where l.LocationName == user.HomePharmacy select lt).Single();

                var query = User.IsInRole(Properties.UserRoles.PharmacyManager) ?
                    "SELECT [UzimaDrug].Id, [UzimaDrug].DrugName AS 'Drug Name', COUNT(*) AS Quantity, [UzimaInventory].ExpirationDate AS 'Expiration Date' " +
                    "FROM UzimaInventory LEFT JOIN [UzimaDrug] ON [UzimaDrug].Id=[UzimaInventory].DrugId " +
                    "WHERE [UzimaInventory].StatusId=2 " +
                    "GROUP BY [UzimaDrug].Id, [UzimaDrug].DrugName, [UzimaInventory].ExpirationDate" :
                    "SELECT [UzimaDrug].Id, [UzimaDrug].DrugName AS 'Drug Name', COUNT(*) AS Quantity, [UzimaInventory].ExpirationDate AS 'Expiration Date' " +
                    "FROM UzimaInventory LEFT JOIN [UzimaDrug] ON [UzimaDrug].Id=[UzimaInventory].DrugId " +
                    $"WHERE [UzimaInventory].StatusId=2 AND [UzimaInventory].CurrentLocationId={homePharmacy.Id} " +
                    "GROUP BY [UzimaDrug].Id, [UzimaDrug].DrugName, [UzimaInventory].ExpirationDate";
                using (var dataSet = ConnectionPool.Query(query, "UzimaDrug", "UzimaInventory"))
                {
                    ViewBag.Columns = dataSet.Tables[0].Columns;
                    ViewBag.Data = dataSet.Tables[0].Rows;
                    return View();
                }
            }
        }

        // GET: Order/DispenseItem
        public ActionResult DispenseItem(int id, int qty)
        {
            using (var uzima = new UzimaRxEntities())
            {
                ViewBag.Quantity = qty;
                var inventory = (from i in uzima.UzimaDrugs where i.Id == id select i).Single();
                ViewBag.Drug = inventory;

                var locations = uzima.UzimaLocations.ToList();
                var users = uzima.AspNetUsers.ToList();
                if (!(locations is null))
                {

                    var userhomelocation = (User.IsInRole(Properties.UserRoles.PharmacyManager)) ?
                        (from location in locations select location) :
                       (from location in locations
                        join user in users on location.LocationName equals user.HomePharmacy
                        where user.Username == User.Identity.Name
                        select location);

                    ViewBag.LocationNeeded = new SelectList(userhomelocation, "Id", "LocationName");
                }

                return View();
            }
        }

        // POST: Order/DispenseItem
        [HttpPost]
        public async Task<ActionResult> DispenseItem(String drugname, String txtQty, UzimaInventory model)
        {

            int id;
            string userid;

            try
            {
                using (var uzima = new UzimaRxEntities())
                {

                    var userhomelocation =
                    (from location in uzima.UzimaLocations
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
                            (from drug in uzima.UzimaInventories
                             where drug.StatusId == 0 && drugname == drug.UzimaDrug.DrugName && drug.FutureLocationId == userhomelocation
                             select drug.Id).FirstOrDefault();


                        var entryToEdit = uzima.UzimaInventories.Find(id);
                        uzima.UzimaInventories.Remove(entryToEdit);
                        await uzima.SaveChangesAsync();

                        entryToEdit.FutureLocationId = model.FutureLocationId;
                        entryToEdit.CurrentLocationId = (int)model.FutureLocationId;
                        entryToEdit.DateOrdered = DateTime.Now;
                        entryToEdit.LastModifiedBy = userid;
                        entryToEdit.StatusId = 3;


                        uzima.UzimaInventories.Add(entryToEdit);
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

            return RedirectToAction("DispenseItem");
        }

        // GET: Order/SelectDispenseOrder
        public ActionResult SelectDispenseOrder()
        {
            AspNetUser user;
            UzimaLocationType homePharmacy;
            using (var uzima = new UzimaRxEntities())
            {
                var userId = User.Identity.GetUserId();
                user = (from u in uzima.AspNetUsers where u.Id == userId select u).Single();
                homePharmacy = (from lt in uzima.UzimaLocationTypes join l in uzima.UzimaLocations on lt.LocationId equals l.Id where l.LocationName == user.HomePharmacy select lt).Single();

                var query = User.IsInRole(Properties.UserRoles.PharmacyManager) ?
                    "SELECT [UzimaDrug].Id, [UzimaDrug].DrugName AS 'Drug Name', COUNT(*) AS Quantity, [UzimaInventory].ExpirationDate AS 'Expiration Date' " +
                    "FROM UzimaInventory LEFT JOIN [UzimaDrug] ON [UzimaDrug].Id=[UzimaInventory].DrugId " +
                    "WHERE [UzimaInventory].StatusId=0 " +
                    "GROUP BY [UzimaDrug].Id, [UzimaDrug].DrugName, [UzimaInventory].ExpirationDate" :
                    "SELECT [UzimaDrug].Id, [UzimaDrug].DrugName AS 'Drug Name', COUNT(*) AS Quantity, [UzimaInventory].ExpirationDate AS 'Expiration Date' " +
                    "FROM UzimaInventory LEFT JOIN [UzimaDrug] ON [UzimaDrug].Id=[UzimaInventory].DrugId " +
                    $"WHERE [UzimaInventory].StatusId=0 AND [UzimaInventory].CurrentLocationId={homePharmacy.Id} AND [UzimaInventory].FutureLocationId={homePharmacy.Id} " +
                    "GROUP BY [UzimaDrug].Id, [UzimaDrug].DrugName, [UzimaInventory].ExpirationDate";
                using (var dataSet = ConnectionPool.Query(query, "UzimaDrug", "UzimaInventory"))
                {
                    ViewBag.Columns = dataSet.Tables[0].Columns;
                    ViewBag.Data = dataSet.Tables[0].Rows;
                    return View();
                }
            }
        }


        // GET: Order/DestroyItem
        public ActionResult DestroyItem(int id, int qty)
        {
            using (var uzima = new UzimaRxEntities())
            {
                ViewBag.Quantity = qty;
                var inventory = (from i in uzima.UzimaDrugs where i.Id == id select i).Single();
                ViewBag.Drug = inventory;

                var locations = uzima.UzimaLocations.ToList();
                var users = uzima.AspNetUsers.ToList();
                if (!(locations is null))
                {

                    var userhomelocation = (User.IsInRole(Properties.UserRoles.PharmacyManager)) ?
                        (from location in locations select location) :
                       (from location in locations
                        join user in users on location.LocationName equals user.HomePharmacy
                        where user.Username == User.Identity.Name
                        select location);

                    ViewBag.LocationNeeded = new SelectList(userhomelocation, "Id", "LocationName");
                }

                return View();
            }
        }


        // POST: Order/DestroyOrder
        [HttpPost]
        public async Task<ActionResult> DestroyItem(String drugname, String txtQty, UzimaInventory model)
        {

            int id;
            string userid;

            try
            {
                using (var uzima = new UzimaRxEntities())
                {

                    var userhomelocation =
                    (from location in uzima.UzimaLocations
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
                            (from drug in uzima.UzimaInventories
                             where drug.StatusId == 0 && drugname == drug.UzimaDrug.DrugName && drug.FutureLocationId == userhomelocation
                             select drug.Id).FirstOrDefault();


                        var entryToEdit = uzima.UzimaInventories.Find(id);
                        uzima.UzimaInventories.Remove(entryToEdit);
                        await uzima.SaveChangesAsync();

                        entryToEdit.FutureLocationId = model.FutureLocationId;
                        entryToEdit.CurrentLocationId = (int)model.FutureLocationId;
                        entryToEdit.DateOrdered = DateTime.Now;
                        entryToEdit.LastModifiedBy = userid;
                        entryToEdit.StatusId = 4;


                        uzima.UzimaInventories.Add(entryToEdit);
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

            return RedirectToAction("DestroyItem");
        }

        // GET: Order/SelectDestroyOrder
        public ActionResult SelectDestroyOrder()
        {
            AspNetUser user;
            UzimaLocationType homePharmacy;
            using (var uzima = new UzimaRxEntities())
            {
                var userId = User.Identity.GetUserId();
                user = (from u in uzima.AspNetUsers where u.Id == userId select u).Single();
                homePharmacy = (from lt in uzima.UzimaLocationTypes join l in uzima.UzimaLocations on lt.LocationId equals l.Id where l.LocationName == user.HomePharmacy select lt).Single();

                var query = User.IsInRole(Properties.UserRoles.PharmacyManager) ?
                    "SELECT [UzimaDrug].Id, [UzimaDrug].DrugName AS 'Drug Name', COUNT(*) AS Quantity, [UzimaInventory].ExpirationDate AS 'Expiration Date' " +
                    "FROM UzimaInventory LEFT JOIN [UzimaDrug] ON [UzimaDrug].Id=[UzimaInventory].DrugId " +
                    "WHERE [UzimaInventory].StatusId=0 " +
                    "GROUP BY [UzimaDrug].Id, [UzimaDrug].DrugName, [UzimaInventory].ExpirationDate" :
                    "SELECT [UzimaDrug].Id, [UzimaDrug].DrugName AS 'Drug Name', COUNT(*) AS Quantity, [UzimaInventory].ExpirationDate AS 'Expiration Date' " +
                    "FROM UzimaInventory LEFT JOIN [UzimaDrug] ON [UzimaDrug].Id=[UzimaInventory].DrugId " +
                    $"WHERE [UzimaInventory].StatusId=0 AND [UzimaInventory].CurrentLocationId={homePharmacy.Id} AND [UzimaInventory].FutureLocationId={homePharmacy.Id} " +
                    "GROUP BY [UzimaDrug].Id, [UzimaDrug].DrugName, [UzimaInventory].ExpirationDate";
                using (var dataSet = ConnectionPool.Query(query, "UzimaDrug", "UzimaInventory"))
                {
                    ViewBag.Columns = dataSet.Tables[0].Columns;
                    ViewBag.Data = dataSet.Tables[0].Rows;
                    return View();
                }
            }
        }
    }
}
