using PTGApplication.Models;
using PTGApplication.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace PTGApplication.Controllers
{
    public class ReportController : Controller
    {
        // GET: Report
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult PendingOrders()
        {
            var query = "select UzimaDrug.DrugName AS 'Drug Name', COUNT(*) AS 'Quantity', CAST(FLOOR(CAST(DateOrdered AS float)) AS datetime) AS 'Ordered On', Username AS 'Ordered By', UzimaLocation.LocationName as 'Going To', ExpirationDate AS 'Expiring On' from[dbo].[UzimaInventory] join UzimaDrug on UzimaDrug.Id = UzimaInventory.DrugId LEFT JOIN AspNetUsers ON AspNetUsers.Id = UzimaInventory.LastModifiedBy LEFT JOIN UzimaLocation ON UzimaLocation.Id = UzimaInventory.FutureLocationId where FutureLocationId  is not null  And StatusId = 1 GROUP BY UzimaDrug.DrugName, CAST(FLOOR(CAST(DateOrdered AS float)) AS datetime), AspNetUsers.Username, UzimaLocation.LocationName, ExpirationDate, FutureLocationId ORDER BY UzimaInventory.FutureLocationId; ";
            using (var dataSet = ConnectionPool.Query(query, "UzimaDrug", "UzimaInventory", "UzimaLocation", "AspNetUsers"))
            {
                ViewBag.Columns = dataSet.Tables[0].Columns;
                ViewBag.Data = dataSet.Tables[0].Rows;

                return View();
            }
        }
        public ActionResult ExpiredDrugs()
        {
            return View();
        }

        public ActionResult Inventory()
        {
            using (var uzima = new UzimaRxEntities())
            {
                var userId =
                    (from user in uzima.AspNetUsers
                     join location in uzima.UzimaLocations on user.HomePharmacy equals location.LocationName
                     where user.Username == User.Identity.Name
                     select location.Id).SingleOrDefault();
                string query = (User.IsInRole(Properties.UserRoles.PharmacyManager)) ?
                    "Select DrugName as 'Drug Name', Count(DrugId) as 'Quantity', " +
                    "LocationName as 'Location Name', ExpirationDate as 'Expiration Date' " +
                    "From UzimaDrug Join UzimaInventory on UzimaDrug.Id = DrugId Join " +
                    "UzimaLocation on UzimaInventory.CurrentLocationId = UzimaLocation.Id Group by " +
                    "LocationName,ExpirationDate,DrugName Order by LocationName, ExpirationDate" :
                    $"Select DrugName as 'Drug Name', Count(DrugId) as 'Quantity', " +
                    $"LocationName as 'Location Name', ExpirationDate as 'Expiration Date' " +
                    $"From UzimaDrug Join UzimaInventory on UzimaDrug.Id = DrugId Join " +
                    $"UzimaLocation on UzimaInventory.CurrentLocationId = UzimaLocation.Id WHERE CurrentLocationId=" +
                    $"{userId} Group by LocationName,ExpirationDate,DrugName Order by LocationName, ExpirationDate";
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