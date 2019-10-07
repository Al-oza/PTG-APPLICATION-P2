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

                var location = new PharmacyLocation()
                {
                    Id = cs.PharmacyLocations.Count(),
                    Name = model.Name,
                    UpstremSupplier = model.UpstremSupplier,
                    IsHospital = model.IsHospital,
                    IsClinic = model.IsClinic,
                    Address = model.Address,
                    Phone = model.Phone
                };

                cs.PharmacyLocations.Add(location);

                cs.SaveChanges();
            }
            catch (Exception ex)
            {
                ViewBag.errorMessage = ex.Message;
                return View("Error");
            }

            return RedirectToAction("LocationAdded");
        }

        // GET: LocationAdded
        public ActionResult LocationAdded()
        {
            return View();
        }
    }
}