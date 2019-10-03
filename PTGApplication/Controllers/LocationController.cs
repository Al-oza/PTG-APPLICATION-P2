using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PTGApplication.Models;

namespace PTGApplication.Controllers
{
    public class LocationController : Controller
    {

        Entities cs = new Entities();
        // GET: Location
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult AddLocation()
        {
            var locations = cs.PharmacyLocations.ToList();
            if (locations != null)
            {
                ViewBag.data = locations;
            }

            return View();
        }
    }
}