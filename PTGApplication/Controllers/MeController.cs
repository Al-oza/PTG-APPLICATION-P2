using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using PTGApplication.Models;
using System.Web;
using System.Web.Http;

namespace PTGApplication.Controllers
{
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