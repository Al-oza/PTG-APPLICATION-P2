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
                var suppliers = cs.PharmacySuppliers.ToList();
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
                        UpstreamSupplier = model.UpstreamSupplier,
                        IsHospital = true,
                        IsClinic = false,
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
        }

        public ActionResult AddClinicLocation()
        {
            using (var cs = new UzimaRxEntities())
            {
                var suppliers = cs.PharmacySuppliers.ToList();
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
                        UpstreamSupplier = model.UpstreamSupplier,
                        IsHospital = false,
                        IsClinic = true,
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
        }
        public ActionResult AddSupplierLocation()
        {

            return View();
        }
        [HttpPost]
        public ActionResult AddSupplierLocation(PharmacySupplier model)
        {

            using (var cs = new UzimaRxEntities())
            {
                try
                {
                    var supplier = new PharmacySupplier()
                    {

                        Id = cs.PharmacySuppliers.Count(),
                        Name = model.Name,
                        Address = model.Address,
                        Phone = model.Phone
                    };

                    cs.PharmacySuppliers.Add(supplier);

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

        public ActionResult InsertLocation(PharmacyLocation model)
        {
            using (var cs = new UzimaRxEntities())
            {
                try
                {
                    var location = new PharmacyLocation()
                    {
                        Id = cs.PharmacyLocations.Count(),
                        Name = model.Name,
                        UpstreamSupplier = model.UpstreamSupplier,
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
        }

        // GET: LocationAdded
        public ActionResult LocationAdded()
        {
            return View();
        }
    }
}