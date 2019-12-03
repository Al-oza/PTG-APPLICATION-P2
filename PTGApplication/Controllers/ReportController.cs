using PTGApplication.Models;
using PTGApplication.Providers;
using System.Linq;
using System.Web.Mvc;

namespace PTGApplication.Controllers
{
    public class ReportController : Controller
    {
        // GET: Report
        public ActionResult Index()
        {
            TempData["UserRoleAdmin"] = User.IsInRole(Properties.UserRoles.PharmacyManager);
            TempData["UserRoleSiteManager"] = User.IsInRole(Properties.UserRoles.CareSiteInventoryManager);
            return View();
        }

        public ActionResult PendingOrders()
        {

            string query;
            if (User.IsInRole(Properties.UserRoles.SystemAdmin))
            {
                query = "select UzimaDrug.DrugName AS 'Drug', COUNT(*) AS 'Quantity', CAST(FLOOR(CAST(DateOrdered AS float)) AS datetime) AS 'Ordered On', Username AS 'Ordered By', UzimaLocation.LocationName as 'Going To', ExpirationDate AS 'Expiring On' from[dbo].[UzimaInventory] join UzimaDrug on UzimaDrug.Id = UzimaInventory.DrugId LEFT JOIN AspNetUsers ON AspNetUsers.Id = UzimaInventory.LastModifiedBy LEFT JOIN UzimaLocation ON UzimaLocation.Id = UzimaInventory.FutureLocationId where FutureLocationId  is not null  And StatusId = 1 GROUP BY UzimaDrug.DrugName, CAST(FLOOR(CAST(DateOrdered AS float)) AS datetime), AspNetUsers.Username, UzimaLocation.LocationName, ExpirationDate, FutureLocationId ORDER BY UzimaInventory.FutureLocationId; ";
            }
            else
            {
                System.Collections.Generic.List<UzimaLocation> locationId;
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
                }
                for (int i = 0; i < locationId.Count(); i++)
                {
                    if (i == locationId.Count() - 1)
                    {
                        locationIds += locationId[i].Id + ")";
                    }
                    else
                    {
                        locationIds += locationId[i].Id + ", ";
                    }
                }
                query = "select UzimaDrug.DrugName AS 'Drug', COUNT(*) AS 'Quantity', " +
                    "CAST(FLOOR(CAST(DateOrdered AS float)) AS datetime) AS 'Ordered On', Username AS 'Ordered By', " +
                    "UzimaLocation.LocationName as 'Going To', ExpirationDate AS 'Expiring On' from[dbo].[UzimaInventory] " +
                    "join UzimaDrug on UzimaDrug.Id = UzimaInventory.DrugId LEFT JOIN AspNetUsers ON AspNetUsers.Id = UzimaInventory.LastModifiedBy " +
                    "LEFT JOIN UzimaLocation ON UzimaLocation.Id = UzimaInventory.FutureLocationId " +
                    $"where FutureLocationId  in {locationIds}  And StatusId = 1 GROUP BY UzimaDrug.DrugName, " +
                    "CAST(FLOOR(CAST(DateOrdered AS float)) AS datetime), AspNetUsers.Username, UzimaLocation.LocationName, ExpirationDate, " +
                    "FutureLocationId ORDER BY UzimaInventory.FutureLocationId; ";
            }

            using (var dataSet = ConnectionPool.Query(query, "UzimaDrug", "UzimaInventory", "UzimaLocation", "AspNetUsers"))
            {
                ViewBag.Columns = dataSet.Tables[0].Columns;
                ViewBag.Data = dataSet.Tables[0].Rows;

                return View();
            }
        }
        public ActionResult ExpiredDrugs()
        {
            using (var uzima = new UzimaRxEntities())
            {
                string query;
                if (User.IsInRole(Properties.UserRoles.SystemAdmin))
                {
                    query = "Select DrugName as 'Drug', Count(DrugId) as 'Quantity',LocationName as 'Current Location', ExpirationDate as " +
                        "'Expired On' from UzimaInventory join UzimaDrug on UzimaDrug.Id = UzimaInventory.DrugId join UzimaLocation on " +
                        "UzimaInventory.CurrentLocationId = UzimaLocation.Id where ExpirationDate <= CURRENT_TIMESTAMP and StatusId IN(0, 1, 2) " +
                        "Group by LocationName,ExpirationDate,DrugName ORDER BY  expirationDate ASC, DrugName, LocationName";
                }
                else
                {
                    System.Collections.Generic.List<UzimaLocation> locationId;
                    string locationIds = "(";
                    var hp = (from location in uzima.UzimaLocations
                              join user in uzima.AspNetUsers on location.LocationName equals user.HomePharmacy
                              where user.Username == User.Identity.Name
                              select location).Single();
                    locationId = (User.IsInRole(Properties.UserRoles.PharmacyManager)) ?
                        (from location in uzima.UzimaLocations
                         join type in uzima.UzimaLocationTypes on location.Id equals type.LocationId
                         where type.LocationId == hp.Id || type.Supplier == hp.Id
                         select location).ToList() :
                         (from location in uzima.UzimaLocations
                          join type in uzima.UzimaLocationTypes on location.Id equals type.LocationId
                          where type.LocationId == hp.Id
                          select location).ToList();

                    for (int i = 0; i < locationId.Count(); i++)
                    {
                        if (i == locationId.Count() - 1)
                        {
                            locationIds += locationId[i].Id + ")";
                        }
                        else
                        {
                            locationIds += locationId[i].Id + ", ";
                        }
                    }
                    query = $"Select DrugName as 'Drug',Count(DrugId) as 'Quantity', LocationName as 'Current Location',ExpirationDate as 'Expired " +
                        $"On' from UzimaInventory join UzimaDrug on UzimaDrug.Id = UzimaInventory.DrugId join UzimaLocation on " +
                        $"UzimaInventory.CurrentLocationId = UzimaLocation.Id where ExpirationDate <= CURRENT_TIMESTAMP and StatusId IN(0, 1, 2) " +
                        $"and CurrentLocationId IN {locationIds} Group by LocationName,ExpirationDate,DrugName ORDER BY  expirationDate ASC, DrugName, LocationName";
                }

                using (var dataSet = ConnectionPool.Query(query, "UzimaDrug", "UzimaInventory", "UzimaLocation", "AspNetUsers"))
                {
                    ViewBag.Columns = dataSet.Tables[0].Columns;
                    ViewBag.Data = dataSet.Tables[0].Rows;

                    return View();
                }
            }
        }

        public ActionResult Inventory()
        {
            using (var uzima = new UzimaRxEntities())
            {
                string query;
                if (User.IsInRole(Properties.UserRoles.SystemAdmin))
                {
                    query =
                        "Select DrugName as 'Drug', Count(DrugId) as 'Quantity', " +
                        "LocationName as 'Location', ExpirationDate as 'Expiration Date' " +
                        "From UzimaDrug Join UzimaInventory on UzimaDrug.Id = DrugId Join " +
                        "UzimaLocation on UzimaInventory.CurrentLocationId = UzimaLocation.Id Group by " +
                        "LocationName,ExpirationDate,DrugName Order by LocationName, ExpirationDate";
                }
                else
                {
                    System.Collections.Generic.List<UzimaLocation> locationId;
                    string locationIds = "(";
                    var hp = (from location in uzima.UzimaLocations
                              join user in uzima.AspNetUsers on location.LocationName equals user.HomePharmacy
                              where user.Username == User.Identity.Name
                              select location).Single();
                    locationId = (User.IsInRole(Properties.UserRoles.PharmacyManager)) ?
                        (from location in uzima.UzimaLocations
                         join type in uzima.UzimaLocationTypes on location.Id equals type.LocationId
                         where type.LocationId == hp.Id || type.Supplier == hp.Id
                         select location).ToList() :
                         (from location in uzima.UzimaLocations
                          join type in uzima.UzimaLocationTypes on location.Id equals type.LocationId
                          where type.LocationId == hp.Id
                          select location).ToList();

                    for (int i = 0; i < locationId.Count(); i++)
                    {
                        if (i == locationId.Count() - 1)
                        {
                            locationIds += locationId[i].Id + ")";
                        }
                        else
                        {
                            locationIds += locationId[i].Id + ", ";
                        }
                    }
                    query = "Select DrugName as 'Drug', Count(DrugId) as 'Quantity', " +
                        "LocationName as 'Location', ExpirationDate as 'Expiration Date' " +
                        "From UzimaDrug Join UzimaInventory on UzimaDrug.Id = DrugId Join " +
                        "UzimaLocation on UzimaInventory.CurrentLocationId = UzimaLocation.Id WHERE CurrentLocationId IN " +
                        $"{locationIds} AND statusId = 0 Group by LocationName,ExpirationDate,DrugName Order by LocationName, ExpirationDate";
                }

                using (var dataSet = ConnectionPool.Query(query, "UzimaDrug", "UzimaInventory", "UzimaLocation", "AspNetUsers"))
                {
                    ViewBag.Columns = dataSet.Tables[0].Columns;
                    ViewBag.Data = dataSet.Tables[0].Rows;

                    return View();
                }
            }
        }

        public ActionResult ExpiringDrugs()
        {
            using (var uzima = new UzimaRxEntities())
            {
                string query;
                if (User.IsInRole(Properties.UserRoles.SystemAdmin))
                {
                    query =
                       "Select DrugName as 'Drug', Count(DrugId) as 'Quantity', LocationName as 'Current Location', ExpirationDate as 'Expiring On' " +
                       "from UzimaInventory " +
                       "join UzimaDrug on UzimaDrug.Id = UzimaInventory.DrugId join UzimaLocation on UzimaInventory.CurrentLocationId = UzimaLocation.Id " +
                       "where ExpirationDate <= dateadd(dd, 120, getdate()) and ExpirationDate > CURRENT_TIMESTAMP and StatusId IN(0, 1, 2)" +
                       " Group by LocationName,ExpirationDate,DrugName " +
                       "ORDER BY  expirationDate ASC, DrugName, LocationName";
                }
                else
                {
                    System.Collections.Generic.List<UzimaLocation> locationId;
                    string locationIds = "(";
                    var hp = (from location in uzima.UzimaLocations
                              join user in uzima.AspNetUsers on location.LocationName equals user.HomePharmacy
                              where user.Username == User.Identity.Name
                              select location).Single();
                    locationId = (User.IsInRole(Properties.UserRoles.PharmacyManager)) ?
                        (from location in uzima.UzimaLocations
                         join type in uzima.UzimaLocationTypes on location.Id equals type.LocationId
                         where type.LocationId == hp.Id || type.Supplier == hp.Id
                         select location).ToList() :
                         (from location in uzima.UzimaLocations
                          join type in uzima.UzimaLocationTypes on location.Id equals type.LocationId
                          where type.LocationId == hp.Id
                          select location).ToList();

                    for (int i = 0; i < locationId.Count(); i++)
                    {
                        if (i == locationId.Count() - 1)
                        {
                            locationIds += locationId[i].Id + ")";
                        }
                        else
                        {
                            locationIds += locationId[i].Id + ", ";
                        }
                    }
                    query = $"Select DrugName as 'Drug', Count(DrugId) as 'Quantity', LocationName as 'Current Location',ExpirationDate as 'Expiring On' from UzimaInventory " +
                       "join UzimaDrug on UzimaDrug.Id = UzimaInventory.DrugId join UzimaLocation on UzimaInventory.CurrentLocationId = UzimaLocation.Id " +
                       "where ExpirationDate <= dateadd(dd, 120, getdate()) and ExpirationDate > CURRENT_TIMESTAMP and StatusId IN(0, 1, 2) " +
                       $"and CurrentLocationId IN {locationIds}" +
                       " Group by LocationName,ExpirationDate,DrugName" +
                       " ORDER BY  expirationDate ASC, DrugName, LocationName";
                }
                using (var dataSet = ConnectionPool.Query(query, "UzimaDrug", "UzimaInventory", "UzimaLocation", "AspNetUsers"))
                {
                    ViewBag.Columns = dataSet.Tables[0].Columns;
                    ViewBag.Data = dataSet.Tables[0].Rows;

                    return View();
                }
            }
        }

        public ActionResult DispensedDrugs()
        {
            using (var uzima = new UzimaRxEntities())
            {
                string query;
                if (User.IsInRole(Properties.UserRoles.SystemAdmin))
                {
                    query = "Select DrugName as 'Drug', COUNT(DrugId) as 'Quantity', LocationName as 'Current Location',CAST(FLOOR(CAST(DateOrdered AS float)) AS datetime) as 'Dispensed On' " +
                        "from UzimaInventory join UzimaDrug on UzimaDrug.Id = UzimaInventory.DrugId join UzimaLocation on " +
                        "UzimaInventory.CurrentLocationId = UzimaLocation.Id where StatusId = 3 Group by LocationName, " +
                        "CAST(FLOOR(CAST(DateOrdered AS float)) AS datetime),  DrugName ORDER BY  'Dispensed On', DrugName, LocationName";
                }
                else
                {
                    System.Collections.Generic.List<UzimaLocation> locationId;
                    string locationIds = "(";
                    var hp = (from location in uzima.UzimaLocations
                              join user in uzima.AspNetUsers on location.LocationName equals user.HomePharmacy
                              where user.Username == User.Identity.Name
                              select location).Single();
                    locationId = (User.IsInRole(Properties.UserRoles.PharmacyManager)) ?
                        (from location in uzima.UzimaLocations
                         join type in uzima.UzimaLocationTypes on location.Id equals type.LocationId
                         where type.LocationId == hp.Id || type.Supplier == hp.Id
                         select location).ToList() :
                         (from location in uzima.UzimaLocations
                          join type in uzima.UzimaLocationTypes on location.Id equals type.LocationId
                          where type.LocationId == hp.Id
                          select location).ToList();

                    for (int i = 0; i < locationId.Count(); i++)
                    {
                        if (i == locationId.Count() - 1)
                        {
                            locationIds += locationId[i].Id + ")";
                        }
                        else
                        {
                            locationIds += locationId[i].Id + ", ";
                        }
                    }
                    query = $"Select DrugName as 'Drug', COUNT(DrugId) as 'Quantity', LocationName as 'Current Location',CAST(FLOOR(CAST(DateOrdered AS float)) AS datetime) as 'Dispensed On' " +
                    $"from UzimaInventory join UzimaDrug on UzimaDrug.Id = UzimaInventory.DrugId join UzimaLocation on UzimaInventory.CurrentLocationId = UzimaLocation.Id " +
                    $"where StatusId = 3 and CurrentLocationId IN {locationIds}" +
                    $"Group by LocationName, CAST(FLOOR(CAST(DateOrdered AS float)) AS datetime),  DrugName" +
                    $" ORDER BY  'Dispensed On', DrugName, LocationName";
                }
                using (var dataSet = ConnectionPool.Query(query, "UzimaDrug", "UzimaInventory", "UzimaLocation", "AspNetUsers"))
                {
                    ViewBag.Columns = dataSet.Tables[0].Columns;
                    ViewBag.Data = dataSet.Tables[0].Rows;

                    return View();
                }
            }
        }

        public ActionResult DestroyedDrugs()

        {
            using (var uzima = new UzimaRxEntities())
            {
                string query;
                if (User.IsInRole(Properties.UserRoles.SystemAdmin))
                {
                    query = "Select DrugName as 'Drug', COUNT(DrugId) as 'Quantity', LocationName as 'Current Location',CAST(FLOOR(CAST(DateOrdered AS float)) AS datetime) as 'Dispensed On' " +
                        "from UzimaInventory join UzimaDrug on UzimaDrug.Id = UzimaInventory.DrugId join UzimaLocation on " +
                        "UzimaInventory.CurrentLocationId = UzimaLocation.Id where StatusId = 4 Group by LocationName, " +
                        "CAST(FLOOR(CAST(DateOrdered AS float)) AS datetime),  DrugName ORDER BY  'Dispensed On', DrugName, LocationName";
                }
                else
                {
                    System.Collections.Generic.List<UzimaLocation> locationId;
                    string locationIds = "(";
                    var hp = (from location in uzima.UzimaLocations
                              join user in uzima.AspNetUsers on location.LocationName equals user.HomePharmacy
                              where user.Username == User.Identity.Name
                              select location).Single();
                    locationId = (User.IsInRole(Properties.UserRoles.PharmacyManager)) ?
                        (from location in uzima.UzimaLocations
                         join type in uzima.UzimaLocationTypes on location.Id equals type.LocationId
                         where type.LocationId == hp.Id || type.Supplier == hp.Id
                         select location).ToList() :
                         (from location in uzima.UzimaLocations
                          join type in uzima.UzimaLocationTypes on location.Id equals type.LocationId
                          where type.LocationId == hp.Id
                          select location).ToList();

                    for (int i = 0; i < locationId.Count(); i++)
                    {
                        if (i == locationId.Count() - 1)
                        {
                            locationIds += locationId[i].Id + ")";
                        }
                        else
                        {
                            locationIds += locationId[i].Id + ", ";
                        }
                    }
                    query = $"Select DrugName as 'Drug', COUNT(DrugId) as 'Quantity', LocationName as 'Current Location',CAST(FLOOR(CAST(DateOrdered AS float)) AS datetime) as 'Dispensed On' " +
                    $"from UzimaInventory join UzimaDrug on UzimaDrug.Id = UzimaInventory.DrugId join UzimaLocation on UzimaInventory.CurrentLocationId = UzimaLocation.Id " +
                    $"where StatusId = 4 and CurrentLocationId IN {locationIds}" +
                    $"Group by LocationName, CAST(FLOOR(CAST(DateOrdered AS float)) AS datetime),  DrugName" +
                    $" ORDER BY  'Dispensed On', DrugName, LocationName";
                }

                using (var dataSet = ConnectionPool.Query(query, "UzimaDrug", "UzimaInventory", "UzimaLocation", "AspNetUsers"))
                {
                    ViewBag.Columns = dataSet.Tables[0].Columns;
                    ViewBag.Data = dataSet.Tables[0].Rows;

                    return View();
                }
            }
        }
    }
}