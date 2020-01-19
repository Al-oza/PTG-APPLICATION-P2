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
    /// <summary>
    /// Controller for Order Placement and Management
    /// </summary>
    public class OrderController : Controller
    {
        /// <summary>
        /// Home Page for Order Management
        /// </summary>
        /// <returns>Order Related Options</returns>
        // GET: Order
        public ActionResult Index()
        {
            TempData["UserRoleAdmin"] = User.IsInRole(Properties.UserRoles.PharmacyManager);
            TempData["UserRoleSiteManager"] = User.IsInRole(Properties.UserRoles.CareSiteInventoryManager);
            TempData["UserRoleSysAdmin"] = User.IsInRole(Properties.UserRoles.SystemAdmin);
            return View();
        }
        /// <summary>
        /// Place a New Order
        /// </summary>
        /// <param name="id">ID of Item to Order</param>
        /// <param name="qty">Quantity of Items to Order</param>
        /// <param name="expiration">Expiration Date of Item to Order</param>
        /// <returns>Order Details Page</returns>
        // GET: Order/PlaceOrder
        public ActionResult PlaceOrder(Guid id, int qty, string expiration)
        {
            using (var uzima = new UzimaRxEntities())
            {
                ViewBag.Quantity = qty;
                var inventory = (from i in uzima.UzimaDrugs where i.Id == id select i).Single();
                ViewBag.Drug = inventory;
                ViewBag.ExpirationDate = expiration;

                var locations = uzima.UzimaLocations.ToList();
                var users = uzima.AspNetUsers.ToList();
                if (!(locations is null))
                {
                    var hp = (from location in uzima.UzimaLocations
                              join user in uzima.AspNetUsers on location.LocationName equals user.HomePharmacy
                              where user.Username == User.Identity.Name
                              select location).Single();
                    var userhomelocation = (User.IsInRole(Properties.UserRoles.SystemAdmin)) ?
                        (from location in locations select location) :
                        (User.IsInRole(Properties.UserRoles.CareSiteInventoryManager)) ?
                       (from location in locations
                        join user in users on location.LocationName equals user.HomePharmacy
                        where user.Username == User.Identity.Name
                        select location) :
                        (from location in uzima.UzimaLocations
                         join type in uzima.UzimaLocationTypes on location.Id equals type.LocationId
                         where type.LocationId == hp.Id || type.Supplier == hp.Id
                         select location).ToList();

                    ViewBag.LocationNeeded = new SelectList(userhomelocation, "Id", "LocationName");
                }

                return View();
            }
        }
        /// <summary>
        /// Write Order to Database
        /// </summary>
        /// <param name="drugname">Name of Item to Order</param>
        /// <param name="txtQty">Number of Items to Order</param>
        /// <param name="txtExpiration">Expiration Date of Item to Order</param>
        /// <param name="model">Information carried over from previous page - handled internally</param>
        /// <returns>Order Placement Confirmation</returns>
        // POST: Order/PlaceOrder
        [HttpPost]
        public async Task<ActionResult> PlaceOrder(String drugname, String txtQty, string txtExpiration, UzimaInventory model)
        {
            Guid id;
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
                             where location.Supplier == null && drug.StatusId == 0 && drugname == drug.UzimaDrug.DrugName && DateTime.Parse(txtExpiration) == drug.ExpirationDate
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

            return RedirectToAction("SelectPlaceOrder");
        }
        /// <summary>
        /// Pick an Inventory Item to Order
        /// </summary>
        /// <returns>List of Inventory Items Available</returns>
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
                    (from l in uzima.UzimaLocations where l.Id == homePharmacy.Id select l).SingleOrDefault()
                   : (from l in uzima.UzimaLocations
                      join lt in uzima.UzimaLocationTypes on l.Id equals lt.LocationId
                      where lt.LocationId == homePharmacy.Supplier
                      select l).SingleOrDefault();

                var query = User.IsInRole(Properties.UserRoles.PharmacyManager) || homeSupplier is null ?
                    "SELECT [UzimaDrug].Id, [UzimaDrug].DrugName AS 'Drug Name', [UzimaDrug].Barcode, COUNT(*) AS Quantity, [UzimaInventory].ExpirationDate AS 'Expiration Date' " +
                    "FROM UzimaInventory LEFT JOIN [UzimaDrug] ON [UzimaDrug].Id=[UzimaInventory].DrugId " +
                    "WHERE [UzimaInventory].StatusId=0 AND [UzimaInventory].FutureLocationId IS NULL " +
                    "GROUP BY [UzimaDrug].Id, [UzimaDrug].DrugName, [UzimaDrug].Barcode, [UzimaInventory].ExpirationDate" :
                    "SELECT [UzimaDrug].Id, [UzimaDrug].DrugName AS 'Drug Name', [UzimaDrug].Barcode, COUNT(*) AS Quantity, [UzimaInventory].ExpirationDate AS 'Expiration Date' " +
                    "FROM UzimaInventory LEFT JOIN [UzimaDrug] ON [UzimaDrug].Id=[UzimaInventory].DrugId " +
                    $"WHERE [UzimaInventory].StatusId=0 AND [UzimaInventory].CurrentLocationId=" +
                    "'" + $"{homeSupplier.Id}" + "' " +
                    "AND [UzimaInventory].FutureLocationId IS NULL " +
                    "GROUP BY [UzimaDrug].Id, [UzimaDrug].DrugName, [UzimaDrug].Barcode, [UzimaInventory].ExpirationDate";
                using (var dataSet = ConnectionPool.Query(query, "UzimaDrug", "UzimaInventory"))
                {
                    ViewBag.Columns = dataSet.Tables[0].Columns;
                    ViewBag.Data = dataSet.Tables[0].Rows;
                    return View();
                }
            }
        }
        /// <summary>
        /// Order Placement Confirmation
        /// </summary>
        /// <returns>Confirmation Page</returns>
        //GET: Order/OrderPlaced
        public ActionResult OrderPlaced()
        {
            return View();
        }
        /// <summary>
        /// Mark an Order as Sent to Orderee
        /// </summary>
        /// <param name="id">ID of Order Shipped</param>
        /// <param name="qty">Number of Items Shipped</param>
        /// <param name="futureLocation">Location of Items Shipped To</param>
        /// <param name="expiration">Expiration of Items Shipped</param>
        /// <returns>Order Sent Confirmation</returns>
        // GET: Order/SendOrder
        public ActionResult SendOrder(Guid id, int qty, string futureLocation, string expiration)
        {
            if (!User.IsInRole(Properties.UserRoles.PharmacyManager))
            {
                Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            }

            using (var uzima = new UzimaRxEntities())
            {
                ViewBag.Quantity = qty;
                var location = (from l in uzima.UzimaLocations where l.LocationName == futureLocation select l).Single();
                var inventory = (from i in uzima.UzimaInventories where i.DrugId == id && i.FutureLocationId == location.Id select i).FirstOrDefault();
                ViewBag.Drug = inventory.UzimaDrug;
                ViewBag.ExpirationDate = expiration;

                var locations = uzima.UzimaLocations.ToList();
                var users = uzima.AspNetUsers.ToList();
                if (!(locations is null))
                {
                    ViewBag.LocationNeeded = location.LocationName;
                }

                return View();
            }
        }
        /// <summary>
        /// Mark Order as Shipped in Database
        /// </summary>
        /// <param name="drugname">Name of Item Shipped</param>
        /// <param name="txtQty">Number of Items Shipped</param>
        /// <param name="futureLocation">Location of Items Shipped to</param>
        /// <param name="txtExpiration">Expiration Date of Items Shipped</param>
        /// <param name="model">Information carried over from previous page - handled internally</param>
        /// <returns>Order Sent Confirmation</returns>
        // POST: Order/SendOrder
        [HttpPost]
        public async Task<ActionResult> SendOrder(String drugname, String txtQty, string futureLocation, string txtExpiration, UzimaInventory model)
        {
            Guid id;
            string userid;
            var expiration = DateTime.Parse(txtExpiration);

            try
            {
                using (var uzima = new UzimaRxEntities())
                {
                    var sendTo = (from l in uzima.UzimaLocations where l.LocationName == futureLocation select l.Id).FirstOrDefault();
                    userid =
                        (from user in uzima.AspNetUsers
                         where user.Username == User.Identity.Name
                         select user.Id).SingleOrDefault();
                    for (int i = 0; i < Convert.ToInt32(txtQty); i++)
                    {

                        id =
                            (from drug in uzima.UzimaInventories
                             join location in uzima.UzimaLocationTypes on drug.CurrentLocationId equals location.LocationId
                             where location.Supplier == null && drug.StatusId == 1 && drugname == drug.UzimaDrug.DrugName && expiration == drug.ExpirationDate
                             select drug.Id).FirstOrDefault();

                        var entryToEdit = uzima.UzimaInventories.Find(id);
                        uzima.UzimaInventories.Remove(entryToEdit);
                        await uzima.SaveChangesAsync();

                        entryToEdit.FutureLocationId = sendTo;
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
        /// <summary>
        /// List of Shipable Orders
        /// </summary>
        /// <returns>List of Placed Orders</returns>
        // GET: Order/SelectSendOrder
        public ActionResult SelectSendOrder()
        {
            System.Collections.Generic.List<UzimaLocation> locationId;
            string query;
            string locationIds = "(";
            using (var uzima = new UzimaRxEntities())
            {
                var hp = (from location in uzima.UzimaLocations
                          join user in uzima.AspNetUsers on location.LocationName equals user.HomePharmacy
                          where user.Username == User.Identity.Name
                          select location).Single();
                locationId = (from location in uzima.UzimaLocations
                              join type in uzima.UzimaLocationTypes on location.Id equals type.LocationId
                              where type.LocationId == hp.Id || type.Supplier == hp.Id
                              select location).ToList();

                for (int i = 0; i < locationId.Count(); i++)
                {
                    if (i == locationId.Count() - 1)
                    {
                        locationIds += ("'" + locationId[i].Id + "')");
                    }
                    else
                    {
                        locationIds += ("'" + locationId[i].Id + "', ");
                    }
                }
                query =
                "SELECT [UzimaDrug].Id, [UzimaDrug].DrugName AS 'Drug Name', [UzimaDrug].Barcode, COUNT(*) AS Quantity, " +
                "[UzimaLocation].LocationName AS 'Send To', [UzimaInventory].ExpirationDate AS 'Expiration Date' FROM UzimaInventory " +
                "LEFT JOIN [UzimaDrug] ON [UzimaDrug].Id=[UzimaInventory].DrugId " +
                "LEFT JOIN [UzimaLocation] ON [UzimaInventory].FutureLocationId=[UzimaLocation].Id " +
                $"WHERE [UzimaInventory].StatusId=1 AND [UzimaInventory].FutureLocationId in {locationIds} " +
                "GROUP BY [UzimaDrug].Id, [UzimaDrug].DrugName, [UzimaDrug].Barcode, [UzimaLocation].LocationName, [UzimaInventory].ExpirationDate";
            }
            using (var dataSet = ConnectionPool.Query(query, "UzimaDrug", "UzimaInventory"))
            {
                ViewBag.Columns = dataSet.Tables[0].Columns;
                ViewBag.Data = dataSet.Tables[0].Rows;
                return View();
            }
        }
        /// <summary>
        /// Order Sent Confirmation
        /// </summary>
        /// <returns>Confirmation Page</returns>
        // GET: OrderSent
        public ActionResult OrderSent()
        {
            return View();
        }
        /// <summary>
        /// Receive an Order
        /// </summary>
        /// <param name="id">ID of Items Received</param>
        /// <param name="qty">Number of Items Received</param>
        /// <param name="expiration">Expiration Date of Items</param>
        /// <returns>Order Received Confirmation</returns>
        // GET: Order/RecieveOrder
        public ActionResult RecieveOrder(Guid id, int qty, string expiration)
        {
            using (var uzima = new UzimaRxEntities())
            {
                ViewBag.Quantity = qty;
                var inventory = (from i in uzima.UzimaDrugs where i.Id == id select i).Single();
                ViewBag.Drug = inventory;
                ViewBag.ExpirationDate = expiration;

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
        /// <summary>
        /// Mark Order as Received in Database
        /// </summary>
        /// <param name="drugname">Name of Item Received</param>
        /// <param name="txtQty">Quantity of Items Received</param>
        /// <param name="txtExpiration">Expiration Date of Items Received</param>
        /// <param name="model">Information carried over from previous page - handled internally</param>
        /// <returns>Order Received Confirmation Page</returns>
        // POST: Order/RecieveOrder
        [HttpPost]
        public async Task<ActionResult> RecieveOrder(String drugname, String txtQty, String txtExpiration, UzimaInventory model)
        {
            Guid id;
            string userid;
            var expiration = DateTime.Parse(txtExpiration);

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
                             where drug.StatusId == 2 && drugname == drug.UzimaDrug.DrugName && drug.FutureLocationId == userhomelocation && drug.ExpirationDate == expiration
                             select drug.Id).FirstOrDefault();

                        var entryToEdit = uzima.UzimaInventories.Find(id);
                        uzima.UzimaInventories.Remove(entryToEdit);
                        await uzima.SaveChangesAsync();

                        entryToEdit.CurrentLocationId = userhomelocation;
                        entryToEdit.FutureLocationId = userhomelocation;
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

            return RedirectToAction("SelectRecieveOrder");
        }
        /// <summary>
        /// List Receivable Items
        /// </summary>
        /// <returns>A list of Receivable Items</returns>
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
                    "SELECT [UzimaDrug].Id, [UzimaDrug].DrugName AS 'Drug Name', [UzimaDrug].Barcode, COUNT(*) AS Quantity, [UzimaInventory].ExpirationDate AS 'Expiration Date' " +
                    "FROM UzimaInventory LEFT JOIN [UzimaDrug] ON [UzimaDrug].Id=[UzimaInventory].DrugId " +
                    "WHERE [UzimaInventory].StatusId=2 " +
                    "GROUP BY [UzimaDrug].Id, [UzimaDrug].DrugName, [UzimaDrug].Barcode, [UzimaInventory].ExpirationDate" :
                    "SELECT [UzimaDrug].Id, [UzimaDrug].DrugName AS 'Drug Name', [UzimaDrug].Barcode, COUNT(*) AS Quantity, [UzimaInventory].ExpirationDate AS 'Expiration Date' " +
                    "FROM UzimaInventory LEFT JOIN [UzimaDrug] ON [UzimaDrug].Id=[UzimaInventory].DrugId " +
                    $"WHERE [UzimaInventory].StatusId=2 AND [UzimaInventory].FutureLocationId=" +
                    "'" + $"{homePharmacy.LocationId}" + "' " +
                    "GROUP BY [UzimaDrug].Id, [UzimaDrug].DrugName, [UzimaDrug].Barcode, [UzimaInventory].ExpirationDate";
                using (var dataSet = ConnectionPool.Query(query, "UzimaDrug", "UzimaInventory"))
                {
                    ViewBag.Columns = dataSet.Tables[0].Columns;
                    ViewBag.Data = dataSet.Tables[0].Rows;
                    return View();
                }
            }
        }
        /// <summary>
        /// Mark an Item as Dispensed
        /// </summary>
        /// <param name="id">ID of Item Dispensed</param>
        /// <param name="qty">Quantity of Items Dispensed</param>
        /// <param name="expiration">Expiration Date of Items Dispensed</param>
        /// <returns>Item Dispensed Confirmation</returns>
        // GET: Order/DispenseItem
        public ActionResult DispenseItem(Guid id, int qty, string expiration)
        {
            using (var uzima = new UzimaRxEntities())
            {
                ViewBag.Quantity = qty;
                var drug = (from i in uzima.UzimaDrugs where i.Id == id select i).Single();
                ViewBag.Drug = drug;
                ViewBag.ExpirationDate = expiration;

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
        /// <summary>
        /// Mark Items as Dispensed in the Database
        /// </summary>
        /// <param name="drugname">Name of Item Dispensed</param>
        /// <param name="txtQty">Quantity of Items Dispensed</param>
        /// <param name="txtExpiration">Expiration Date of Items Dispensed</param>
        /// <param name="model">Information carried over from previous page - handled internally</param>
        /// <returns>Item Dispensed Confirmation</returns>
        // POST: Order/DispenseItem
        [HttpPost]
        public async Task<ActionResult> DispenseItem(String drugname, String txtQty, String txtExpiration, UzimaInventory model)
        {
            Guid id;
            string userid;
            var expiration = DateTime.Parse(txtExpiration);

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

                        id = (User.IsInRole(Properties.UserRoles.PharmacyManager)) ?
                            (from drug in uzima.UzimaInventories
                             where drug.StatusId == 0 && drugname == drug.UzimaDrug.DrugName && expiration == drug.ExpirationDate
                             select drug.Id).FirstOrDefault()
                        : (from drug in uzima.UzimaInventories
                           where drug.StatusId == 0 && drugname == drug.UzimaDrug.DrugName && expiration == drug.ExpirationDate
                           select drug.Id).FirstOrDefault();


                        var entryToEdit = uzima.UzimaInventories.Find(id);
                        uzima.UzimaInventories.Remove(entryToEdit);
                        await uzima.SaveChangesAsync();

                        entryToEdit.FutureLocationId = userhomelocation;
                        entryToEdit.CurrentLocationId = userhomelocation;
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

            return RedirectToAction("SelectDispenseOrder");
        }
        /// <summary>
        /// Select Items to Dispense
        /// </summary>
        /// <returns>List of Dispensible Items</returns>
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
                    "SELECT [UzimaDrug].Id, [UzimaDrug].DrugName AS 'Drug Name', [UzimaDrug].Barcode, COUNT(*) AS Quantity, [UzimaInventory].ExpirationDate AS 'Expiration Date' " +
                    "FROM UzimaInventory LEFT JOIN [UzimaDrug] ON [UzimaDrug].Id=[UzimaInventory].DrugId " +
                    "WHERE [UzimaInventory].StatusId=0 " +
                    "GROUP BY [UzimaDrug].Id, [UzimaDrug].DrugName, [UzimaDrug].Barcode, [UzimaInventory].ExpirationDate" :
                    "SELECT [UzimaDrug].Id, [UzimaDrug].DrugName AS 'Drug Name', [UzimaDrug].Barcode, COUNT(*) AS Quantity, [UzimaInventory].ExpirationDate AS 'Expiration Date' " +
                    "FROM UzimaInventory LEFT JOIN [UzimaDrug] ON [UzimaDrug].Id=[UzimaInventory].DrugId " +
                    $"WHERE [UzimaInventory].StatusId=0 AND [UzimaInventory].CurrentLocationId=" +
                    "'" + $"{homePharmacy.LocationId}" + "' " +
                    "GROUP BY [UzimaDrug].Id, [UzimaDrug].DrugName, [UzimaDrug].Barcode, [UzimaInventory].ExpirationDate";
                using (var dataSet = ConnectionPool.Query(query, "UzimaDrug", "UzimaInventory"))
                {
                    ViewBag.Columns = dataSet.Tables[0].Columns;
                    ViewBag.Data = dataSet.Tables[0].Rows;
                    return View();
                }
            }
        }
        /// <summary>
        /// Destroy an Item
        /// </summary>
        /// <param name="id">ID of Item to Destroy</param>
        /// <param name="qty">Quantity of Items to Destroy</param>
        /// <param name="expiration">Expiration Date of Items to Destroy</param>
        /// <returns>Item Destroyed Confirmation</returns>
        // GET: Order/DestroyItem
        public ActionResult DestroyItem(Guid id, int qty, string expiration)
        {
            using (var uzima = new UzimaRxEntities())
            {
                ViewBag.Quantity = qty;
                var drug = (from i in uzima.UzimaDrugs where i.Id == id select i).Single();
                ViewBag.Drug = drug;
                ViewBag.ExpirationDate = expiration;

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
        /// <summary>
        /// Mark an Item as Destroyed in the Database
        /// </summary>
        /// <param name="drugname">Name of Item to Destroy</param>
        /// <param name="txtQty">Quantity of Items to Destroy</param>
        /// <param name="txtExpiration">Expiration Date of Item to Destroy</param>
        /// <param name="model">Information carried over from previous page - handled internally</param>
        /// <returns>Item Destroyed Confirmation</returns>
        // POST: Order/DestroyOrder
        [HttpPost]
        public async Task<ActionResult> DestroyItem(String drugname, String txtQty, String txtExpiration, UzimaInventory model)
        {
            Guid id;
            string userid;
            var expiration = DateTime.Parse(txtExpiration);

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

                        id = (User.IsInRole(Properties.UserRoles.PharmacyManager)) ?
                            (from drug in uzima.UzimaInventories
                             where drug.StatusId == 0 && drugname == drug.UzimaDrug.DrugName && expiration == drug.ExpirationDate
                             select drug.Id).FirstOrDefault()
                        : (from drug in uzima.UzimaInventories
                           where drug.StatusId == 0 && drugname == drug.UzimaDrug.DrugName && expiration == drug.ExpirationDate
                           select drug.Id).FirstOrDefault();


                        var entryToEdit = uzima.UzimaInventories.Find(id);
                        uzima.UzimaInventories.Remove(entryToEdit);
                        await uzima.SaveChangesAsync();

                        entryToEdit.FutureLocationId = userhomelocation;
                        entryToEdit.CurrentLocationId = userhomelocation;
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

            return RedirectToAction("SelectDispenseOrder");
        }
        /// <summary>
        /// List Destroyable Items
        /// </summary>
        /// <returns>A List of Items that can be Destroyed</returns>
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
                    "SELECT [UzimaDrug].Id, [UzimaDrug].DrugName AS 'Drug Name', [UzimaDrug].Barcode, COUNT(*) AS Quantity, [UzimaInventory].ExpirationDate AS 'Expiration Date' " +
                    "FROM UzimaInventory LEFT JOIN [UzimaDrug] ON [UzimaDrug].Id=[UzimaInventory].DrugId " +
                    "WHERE [UzimaInventory].StatusId=0 " +
                    "GROUP BY [UzimaDrug].Id, [UzimaDrug].DrugName, [UzimaDrug].Barcode, [UzimaInventory].ExpirationDate" :
                     "SELECT [UzimaDrug].Id, [UzimaDrug].DrugName AS 'Drug Name', [UzimaDrug].Barcode, COUNT(*) AS Quantity, [UzimaInventory].ExpirationDate AS 'Expiration Date' " +
                    "FROM UzimaInventory LEFT JOIN [UzimaDrug] ON [UzimaDrug].Id=[UzimaInventory].DrugId " +
                    $"WHERE [UzimaInventory].StatusId=0 AND [UzimaInventory].CurrentLocationId=" +
                    "'" + $"{homePharmacy.LocationId}" + "' " +
                    "GROUP BY [UzimaDrug].Id, [UzimaDrug].DrugName, [UzimaDrug].Barcode, [UzimaInventory].ExpirationDate";
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
