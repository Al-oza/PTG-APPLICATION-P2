using Microsoft.AspNet.Identity;
using PTGApplication.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
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
                     join types in uzima.UzimaLocationTypes on location.Id equals types.LocationId
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

                        Id = Guid.NewGuid(),
                        LocationName = model.LocationName,
                        Address = model.Address,
                        Phone = model.Phone
                    };

                    var type = new UzimaLocationType()
                    {
                        Id = Guid.NewGuid(),
                        LocationType = "Hospital",
                        LocationId = location.Id,
                        Supplier = Guid.Parse(Request.Form["Supplier"])
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
                     join types in uzima.UzimaLocationTypes on location.Id equals types.LocationId
                     where types.Supplier == null || types.LocationType == "hospital"
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

                        Id = Guid.NewGuid(),
                        LocationName = model.LocationName,
                        Address = model.Address,
                        Phone = model.Phone
                    };

                    var type = new UzimaLocationType()
                    {
                        Id = Guid.NewGuid(),
                        LocationType = "Clinic",
                        LocationId = location.Id,
                        Supplier = Guid.Parse(Request.Form["Supplier"])
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

                        Id = Guid.NewGuid(),
                        LocationName = model.LocationName,
                        Address = model.Address,
                        Phone = model.Phone
                    };

                    var type = new UzimaLocationType()
                    {
                        Id = Guid.NewGuid(),
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
        // GET: RequestLocation
        public ActionResult RequestLocation()
        {
            return View();
        }

        // POST
        [HttpPost]
        public async Task<ActionResult> RequestLocation(string name, string address, string phone, string type, string supplier)
        {
            var msg = new IdentityMessage
            {
                Destination = Properties.SharedResources.Email,
                Body = $"Dear SysAdmin,<br /><br />" +
                $"A request for a new {type} location has been submitted:<br />" +
                $"Please add a new {type} location with the details that follow.<br />" +
                $"<pre> Name: {name}" +
                $" Address: {address}" +
                $" Phone: {phone}" +
                $" Supplier: {((supplier == "supplier") ? "NULL" : supplier)}</pre>",
                Subject = "New Location Requested"
            };
            await new EmailService().SendAsync(msg);
            return RedirectToAction("Index", "Home");
        }
    }
}