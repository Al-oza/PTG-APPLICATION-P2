﻿using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using PTGApplication.Models;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace PTGApplication.Controllers
{
    public class AdministrationController : Controller
    {
        // GET: Administration
        public ActionResult Index()
        {
            if (!User.IsInRole(Properties.UserRoles.PharmacyManager))
            {
                Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            }

            return View();
        }

        // GET: Administration/Create
        public ActionResult AddUser()
        {
            if (!User.IsInRole(Properties.UserRoles.PharmacyManager))
            {
                Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            }
            using (var uzima = new UzimaRxEntities())
            {
                var locations =
                    (from location in uzima.PharmacyLocations
                     join type in uzima.PharmacyLocationTypes on location.Id equals type.LocationId
                     where type.Supplier != null
                     select location).ToList();

                var roles = uzima.AspNetRoles.ToList();

                if (!(locations is null))
                {
                    ViewBag.Locations = new SelectList(locations, "Name", "Name");
                }

                if (!(roles is null))
                {
                    ViewBag.Roles = new SelectList(roles, "Name", "Name");
                }

                return View();
            }
        }

        // POST: Administration/AddUser
        [HttpPost]
        public async Task<ActionResult> AddUser(string ddlRoles, string ddlLocations, RegisterViewModel model)
        {
            if (!User.IsInRole(Properties.UserRoles.PharmacyManager))
            {
                Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            }

            var user = new ApplicationUser
            {
                UserName = model.Username,
                Email = model.Email,
                HomePharmacy = ddlLocations,
                Name = model.Name
            };

            var UserManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();

            if (UserManager.FindByEmail(model.Email) != null)
            {
                var callback = UserManager.GeneratePasswordResetToken(model.Username);
                ViewBag.errorMessage = "This email address is already associated with an account. " +
                    $"Maybe you <a href=\"{callback}\">Forgot your password</a>?";
                return View("Error");
            }

            if (UserManager.FindByName(model.Username) != null)
            {
                ViewBag.errorMessage = "This username is already taken, please try again";
                return View("Error");
            }

            var password = Membership.GeneratePassword(12, 1);
            var result = await UserManager.CreateAsync(user, password);
            if (result.Succeeded)
            {
                // await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);

                await UserManager.AddToRoleAsync(user.Id, ddlRoles);

                // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=320771
                // Send an email with this link

                var code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                var callback = Url.Action("ConfirmEmail", "Account",
                    new { user.Id, code }, protocol: Request.Url.Scheme);
                var msgBody = $"Dear {user.Name},<br /><br />" +
                    $"Account Information:<br />   Username: {user.UserName}<br />   Password: {password}<br />" +
                    $"Please confirm your account by clicking <a href=\"{callback}\">here</a>.";
                await UserManager.SendEmailAsync(user.Id, "Confirm Your UzimaRx Account", msgBody);

                // Uncomment to debug locally
                // TempData["ViewBagLink"] = callbackUrl;

                ModelState.Clear();
                return RedirectToAction("AddUser");
            }


            ViewBag.errorMessage = "Something went wrong";
            return View("Error");
        }

        // GET: Administration/Modify/5
        public ActionResult Modify(int id)
        {
            if (!User.IsInRole(Properties.UserRoles.PharmacyManager))
            {
                Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            }
            return View();
        }

        // POST: Administration/Modify/5
        [HttpPost]
        public ActionResult Modify(int id, RegisterViewModel model)
        {
            if (!User.IsInRole(Properties.UserRoles.PharmacyManager))
            {
                Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            }
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Administration/Remove/5
        public ActionResult Remove(int id)
        {
            if (!User.IsInRole(Properties.UserRoles.PharmacyManager))
            {
                Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            }
            return View();
        }

        // POST: Administration/Remove/5
        [HttpPost]
        public ActionResult Remove(int id, ApplicationUser model)
        {
            if (!User.IsInRole(Properties.UserRoles.PharmacyManager))
            {
                Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            }
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
        private async Task<String> SendEmailConfirmationTokenAsync(string userID, string subject, ApplicationUserManager UserManager)
        {
            string code = await UserManager.GenerateEmailConfirmationTokenAsync(userID);
            var callback = Url.Action("ConfirmEmail", "Account",
                new { userID, code }, protocol: Request.Url.Scheme);
            await UserManager.SendEmailAsync(userID, subject,
                $"Please confirm your account by clicking <a href=\"{callback}\">here</a>.");

            return callback;
        }
    }
}
