using PTGApplication.Models;
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
            using (var uzima = new UzimaRxEntities())
            {
                var pendingOrders =
                /*
                    (from i in uzima.PharmacyInventories
                     join u in uzima.AspNetUsers on i.UserId equals u.Id
                     join d in uzima.PharmacyDrugs on i.DrugId equals d.Id
                     join l in uzima.PharmacyLocations on i.FutureLocationId equals l.Id
                     where i.FutureLocationId != null && i.StatusId == 1
                     group new { i, u, d, l } by new { d.DrugName, i.DateOrdered, u.Username,
                         l.LocationName, i.ExpirationDate, i.FutureLocationId }).ToList();*/
                         ReportHelper.Query($"select PharmacyDrug.DrugName AS 'Drug Name', COUNT(*) AS 'Quantity', CAST(FLOOR(CAST(DateOrdered AS float)) AS datetime) AS 'Ordered On', Username AS 'Ordered By', PharmacyLocation.LocationName as 'Going To', ExpirationDate AS 'Expiring On' from[dbo].[PharmacyInventory] join PharmacyDrug on PharmacyDrug.Id = PharmacyInventory.DrugId LEFT JOIN AspNetUsers ON AspNetUsers.Id = PharmacyInventory.UserId LEFT JOIN PharmacyLocation ON PharmacyLocation.Id = PharmacyInventory.FutureLocationId where FutureLocationId is not null  And StatusId = 1 GROUP BY PharmacyDrug.DrugName, CAST(FLOOR(CAST(DateOrdered AS float)) AS datetime), AspNetUsers.Username, PharmacyLocation.LocationName, ExpirationDate, FutureLocationId ORDER BY PharmacyInventory.FutureLocationId;");

                if (!(pendingOrders is null))
                {
                   /* var drugNames = new List<String>();
                    for (int i = 0; i < pendingOrders.Count; i++)
                    {
                        drugNames.Add(pendingOrders[i].Key.DrugName);
                    }
                    ViewBag.DrugNames = drugNames;*/
                }

                return View();
            }
        }

        public ActionResult ExpiredDrugs()
        {
            return View();
        }

        public ActionResult Inventory()
        {
            return View();
        }
    }
}