using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using PTGApplication.Models;
using System;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace PTGApplication.Controllers
{
    public class AdministrationController : Controller
    {
        // GET: Administration/Details
        public ActionResult Details(string id, AspNetUser model)
        {
            using (var uzima = new UzimaRxEntities())
            {
                return View((from user in uzima.AspNetUsers
                             where user.Id == id
                             select user).SingleOrDefault());
            }
        }
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
                    (from location in uzima.UzimaLocations
                     join type in uzima.UzimaLocationTypes on location.Id equals type.LocationId
                     select location).ToList();

                var roles = uzima.AspNetRoles.ToList();

                if (!(locations is null))
                {
                    ViewBag.Locations = new SelectList(locations, "LocationName", "LocationName");
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
                Name = model.Name,
                IsActive = true
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

            string password;
            do
            {
                password = Membership.GeneratePassword(20, 5);
            } while (!Regex.IsMatch(password, "\\d"));

            var result = await UserManager.CreateAsync(user, password);
            if (result.Succeeded)
            {
                // await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);

                await UserManager.AddToRoleAsync(user.Id, ddlRoles);

                // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=320771
                // Send an email with this link

                var code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                var callback = Url.Action("ConfirmEmail", "Account",
                    new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                var msgBody = $"Dear {user.Name},<br /><br />" +
                    $"Account Information:<br />   Username: {user.UserName}<br />   Password: {password}<br />" +
                    $"Please confirm your account by clicking <a href=\"{callback}\">here</a>.";
                await UserManager.SendEmailAsync(user.Id, "Confirm Your UzimaRx Account", msgBody);

                // Uncomment to debug locally
                //TempData["ViewBagLink"] = callback;

                ModelState.Clear();
                return RedirectToAction("UserAdded");
            }


            ViewBag.errorMessage = "Something went wrong";
            return View("Error");
        }

        //GET: UserAdded
        public ActionResult UserAdded()
        {
            return View();
        }

        // GET: Administration/Modify/5
        public ActionResult Modify(string id)
        {
            if (!User.IsInRole(Properties.UserRoles.PharmacyManager))
            {
                Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            }
            using (var uzima = new UzimaRxEntities())
            {
                var locations = (from sites in uzima.UzimaLocations
                                 join types in uzima.UzimaLocationTypes on sites.Id equals types.LocationId
                                 where types.Supplier != null
                                 select sites).ToList();

                var roles = uzima.AspNetRoles.ToList();

                if (!(locations is null))
                {
                    var currentLocation = (from loc in uzima.UzimaLocations
                                           join usr in uzima.AspNetUsers on loc.LocationName equals usr.HomePharmacy
                                           select loc.LocationName).FirstOrDefault();
                    ViewBag.Locations = new SelectList(locations, "LocationName", "LocationName", currentLocation);
                }

                if (!(roles is null))
                {
                    using (var context = new ApplicationDbContext())
                    using (var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context)))
                    {
                        var currentRole = userManager.GetRoles(id).SingleOrDefault();
                        ViewBag.Roles = new SelectList(roles, "Name", "Name", currentRole);
                    }
                }

                return View((from user in uzima.AspNetUsers
                             where user.Id == id
                             select user).SingleOrDefault());
            }
        }

        // POST: Administration/Modify/5
        [HttpPost]
        public async Task<ActionResult> Modify(string ddlRoles, string id, AspNetUser model)
        {
            if (!User.IsInRole(Properties.UserRoles.PharmacyManager))
            {
                Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            }
            try
            {
                using (var uzima = new UzimaRxEntities())
                {

                    var user = (from u in uzima.AspNetUsers
                                where u.Id == id
                                select u).SingleOrDefault();
                    uzima.AspNetUsers.Remove(user);
                    model.PasswordHash = user.PasswordHash;
                    model.SecurityStamp = user.SecurityStamp;

                    uzima.AspNetUsers.Add(model);

                    await uzima.SaveChangesAsync();
                    ViewBag.successMessage = "User Modified";

                    using (var context = new ApplicationDbContext())
                    using (var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context)))
                    {
                        var newUserId = (from u in uzima.AspNetUsers
                                         where u.Username == model.Username
                                         select u.Id).SingleOrDefault();
                        await userManager.AddToRoleAsync(newUserId, ddlRoles);
                    }
                }
            }
            catch
            {
                ViewBag.errorMessage = "Something went wrong";
                return View("Error");
            }
            return RedirectToAction("SelectUser");
        }

        // GET: Administration/Remove/5
        public ActionResult Remove(string id)
        {
            if (!User.IsInRole(Properties.UserRoles.PharmacyManager))
            {
                Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            }
            using (var uzima = new UzimaRxEntities())
            {
                return View((from user in uzima.AspNetUsers
                             where user.Id == id
                             select user).SingleOrDefault());
            }
        }

        // POST: Administration/Remove/5
        [HttpPost]
        public async Task<ActionResult> Remove(string id, ApplicationUser model)
        {
            if (!User.IsInRole(Properties.UserRoles.PharmacyManager))
            {
                Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            }
            try
            {
                var UserManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
                var user = await UserManager.FindByIdAsync(id);
                await UserManager.DeleteAsync(user);
            }
            catch
            {
                ViewBag.errorMessage = "Something went wrong";
                return View("Error");
            }
            return RedirectToAction("SelectUser");
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

        // GET: Administration/SelectUser
        public ActionResult SelectUser()
        {
            using (var uzima = new UzimaRxEntities())
            {
                return View(uzima.AspNetUsers.ToList());
            }
        }
    }
}
