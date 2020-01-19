using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using PTGApplication.Models;
using System.Web;
using System.Web.Http;

namespace PTGApplication.Controllers
{
    /// <summary>
    /// This class is generated. It is used to manage user sign ins and handles login cookies.
    /// </summary>
    /// <remarks cref="https://docs.microsoft.com/en-us/aspnet/mvc/overview/older-versions-1/controllers-and-routing/aspnet-mvc-controllers-overview-cs" />
    [Authorize]
    public class MeController : ApiController
    {
        private ApplicationUserManager _userManager;

        public MeController()
        {
        }

        public MeController(ApplicationUserManager userManager)
        {
            UserManager = userManager;
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        // GET api/Me
        public GetViewModel Get()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            return new GetViewModel() { Pharmacy = user.HomePharmacy, Username = user.UserName };
        }
    }
}