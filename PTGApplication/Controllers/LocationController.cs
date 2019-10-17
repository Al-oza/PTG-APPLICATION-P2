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

        public ActionResult AddHospitalLocation()
        {
            using (var cs = new UzimaRxEntities())
            { 
                var type = cs.PharmacyLocationTypes.Where(t => t.Supplier == null);
                var suppliers = new List<PharmacyLocation>();

                foreach (var supplier in type)
                { suppliers.Add(cs.PharmacyLocations.Find(supplier.Id)); }

                if (suppliers != null)
                {
                    ViewBag.suppliers = suppliers;
                }

                return View();
            }
        }
        [HttpPost]
        public ActionResult AddHospitalLocation(PharmacyLocation model)
        {

            using (var cs = new UzimaRxEntities())
            {
                try
                {
                    var location = new PharmacyLocation()
                    {

                        Id = cs.PharmacyLocations.Count(),
                        Name = model.Name,
                        Address = model.Address,
                        Phone = model.Phone
                    };

                    var type = new PharmacyLocationType()
                    {
                        Id = cs.PharmacyLocationTypes.Count(),
                        LocationType = "Hospital",
                        LocationId = location.Id,
                        Supplier = Convert.ToInt32(Request.Form["Supplier"])
                    };

                    cs.PharmacyLocations.Add(location);
                    cs.PharmacyLocationTypes.Add(type);

                    cs.SaveChanges();
                }
                catch (Exception ex)
                {
                    ViewBag.errorMessage = ex.Message;
                    return View("Error");
                }

                return RedirectToAction("LocationAdded");
            }
        }

        public ActionResult AddClinicLocation()
        {
            using (var cs = new UzimaRxEntities())
            {
                var type = cs.PharmacyLocationTypes.Where(t => t.Supplier == null);
                var suppliers = new List<PharmacyLocation>();

                foreach (var supplier in type)
                { suppliers.Add(cs.PharmacyLocations.Find(supplier.Id)); }

                if (suppliers != null)
                {
                    ViewBag.suppliers = suppliers;
                }

                return View();
            }
        }
        [HttpPost]
        public ActionResult AddClinicLocation(PharmacyLocation model)
        {

            using (var cs = new UzimaRxEntities())
            {
                try
                {
                    var location = new PharmacyLocation()
                    {

                        Id = cs.PharmacyLocations.Count(),
                        Name = model.Name,
                        Address = model.Address,
                        Phone = model.Phone
                    };

                    var type = new PharmacyLocationType()
                    {
                        Id = cs.PharmacyLocationTypes.Count(),
                        LocationType = "Clinic",
                        LocationId = location.Id,
                        Supplier = Convert.ToInt32(Request.Form["Supplier"])
                    };

                    cs.PharmacyLocations.Add(location);
                    cs.PharmacyLocationTypes.Add(type);

                    cs.SaveChanges();
                }
                catch (Exception ex)
                {
                    ViewBag.errorMessage = ex.Message;
                    return View("Error");
                }

                return RedirectToAction("LocationAdded");
            }
        }
        public ActionResult AddSupplierLocation()
        {

            return View();
        }
        [HttpPost]
        public ActionResult AddSupplierLocation(PharmacyLocation model)
        {

            using (var cs = new UzimaRxEntities())
            {
                try
                {
                    var location = new PharmacyLocation()
                    {

                        Id = cs.PharmacyLocations.Count(),
                        Name = model.Name,
                        Address = model.Address,
                        Phone = model.Phone
                    };

                    var type = new PharmacyLocationType()
                    {
                        Id = cs.PharmacyLocationTypes.Count(),
                        LocationType = "Supplier",
                        LocationId = location.Id,
                        Supplier = null
                    };

                    cs.PharmacyLocations.Add(location);
                    cs.PharmacyLocationTypes.Add(type);

                    cs.SaveChanges();
                }
                catch (Exception ex)
                {
                    ViewBag.errorMessage = ex.Message;
                    return View("Error");
                }

                return RedirectToAction("LocationAdded");
            }
        }

        // GET: LocationAdded
        public ActionResult LocationAdded()
        {
            return View();
        }
    }
}