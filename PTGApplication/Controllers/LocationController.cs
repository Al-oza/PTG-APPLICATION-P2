using PTGApplication.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace PTGApplication.Controllers
{
    public class LocationController : Controller
    {
        // GET: Location
        public ActionResult Index()
        {
            return View();
        }

        // GET: Location/AddHospitalLocation
        public ActionResult AddHospitalLocation()
        {
            using (var uzima = new UzimaRxEntities())
            {
                var suppliers =
                    (from location in uzima.UzimaLocations
                     join types in uzima.UzimaLocationTypes on location.Id equals types.Id
                     where types.Supplier == null
                     select location).ToList();

                if (!(suppliers is null))
                {
                    ViewBag.Suppliers = new SelectList(suppliers, "Id", "LocationName");
                }

                return View();
            }
        }

        // POST: Location/AddHospitalLocation
        [HttpPost]
        public ActionResult AddHospitalLocation(UzimaLocation model)
        {
            using (var cs = new UzimaRxEntities())
            {
                try
                {
                    var location = new UzimaLocation()
                    {

                        Id = cs.UzimaLocations.Count(),
                        LocationName = model.LocationName,
                        Address = model.Address,
                        Phone = model.Phone
                    };

                    var type = new UzimaLocationType()
                    {
                        Id = cs.UzimaLocationTypes.Count(),
                        LocationType = "Hospital",
                        LocationId = location.Id,
                        Supplier = Convert.ToInt32(Request.Form["Supplier"])
                    };

                    cs.UzimaLocations.Add(location);
                    cs.UzimaLocationTypes.Add(type);

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

        // GET: Location/AddClinicLocation
        public ActionResult AddClinicLocation()
        {
            using (var uzima = new UzimaRxEntities())
            {
                var suppliers =
                    (from location in uzima.UzimaLocations
                     join types in uzima.UzimaLocationTypes on location.Id equals types.Id
                     where types.Supplier == null
                     select location).ToList();

                if (!(suppliers is null))
                {
                    ViewBag.Suppliers = new SelectList(suppliers, "Id", "LocationName");
                }

                return View();
            }
        }

        // POST: Location/AddClinicLocation
        [HttpPost]
        public ActionResult AddClinicLocation(UzimaLocation model)
        {
            using (var cs = new UzimaRxEntities())
            {
                try
                {
                    var location = new UzimaLocation()
                    {

                        Id = cs.UzimaLocations.Count(),
                        LocationName = model.LocationName,
                        Address = model.Address,
                        Phone = model.Phone
                    };

                    var type = new UzimaLocationType()
                    {
                        Id = cs.UzimaLocationTypes.Count(),
                        LocationType = "Clinic",
                        LocationId = location.Id,
                        Supplier = Convert.ToInt32(Request.Form["Supplier"])
                    };

                    cs.UzimaLocations.Add(location);
                    cs.UzimaLocationTypes.Add(type);

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

        // GET: Location/AddSupplierLocation
        public ActionResult AddSupplierLocation()
        {
            return View();
        }

        // POST: Location/AddSupplierLocation
        [HttpPost]
        public ActionResult AddSupplierLocation(UzimaLocation model)
        {
            using (var cs = new UzimaRxEntities())
            {
                try
                {
                    var location = new UzimaLocation()
                    {

                        Id = cs.UzimaLocations.Count(),
                        LocationName = model.LocationName,
                        Address = model.Address,
                        Phone = model.Phone
                    };

                    var type = new UzimaLocationType()
                    {
                        Id = cs.UzimaLocationTypes.Count(),
                        LocationType = "Supplier",
                        LocationId = location.Id,
                        Supplier = null
                    };

                    cs.UzimaLocations.Add(location);
                    cs.UzimaLocationTypes.Add(type);

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