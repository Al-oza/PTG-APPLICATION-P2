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

       
        // GET: Location
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult AddLocation()
        {
            var cs = new UzimaRxEntities();
            var locations = cs.PharmacyLocations.ToList();
            if (locations != null)
            {
                ViewBag.data = locations;
            }

            return View();
        }

        public ActionResult InsertLocation(PharmacyLocation model)
        {
            try
            {
                var cs = new UzimaRxEntities();

                PharmacyLocation location = new PharmacyLocation();
 
                location.Name = model.Name;
                location.UpstremSupplier = model.UpstremSupplier;
                location.IsHospital = model.IsHospital;
                location.IsClinic = model.IsClinic;
                location.Address = model.Address;
                location.Phone = model.Phone;

                cs.PharmacyLocations.Add(location);

                cs.SaveChanges();

            }
            catch (Exception ex)
            {
                throw ex;
            }

            return RedirectToAction("AddLocation");
        }
    }
}