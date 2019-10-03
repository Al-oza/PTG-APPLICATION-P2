using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin;
using Owin;
using PTGApplication.Models;
using System.Configuration;
using System.Threading.Tasks;
using System.Web.Configuration;

[assembly: OwinStartup(typeof(PTGApplication.Startup))]

namespace PTGApplication
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var configuration = WebConfigurationManager.OpenWebConfiguration("~");
            var section = (ConnectionStringsSection)configuration.GetSection("connectionStrings");
            section.ConnectionStrings["DefaultConnection"].ConnectionString = Properties.SharedResources.DbConnection;

            ConfigureAuth(app);
            CreateRolesAndUsers().Wait();
        }
        private async Task CreateRolesAndUsers()
        {
            var context = new ApplicationDbContext();
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));

            var admin = Properties.UserRoles.PharmacyManager;
            if (!roleManager.RoleExists(admin))
            {
                await roleManager.CreateAsync(new IdentityRole() { Name = admin });

                var usrRapula = new ApplicationUser()
                {
                    EmailConfirmed = true,
                    Email = "rapula@uzima.org",
                    UserName = "rapula"
                };

                var usrBatman = new ApplicationUser()
                {
                    EmailConfirmed = true,
                    Email = "brucewayne@gothem.net",
                    UserName = "batman"
                };

                var chkUser = await userManager.CreateAsync(usrRapula, Properties.SharedResources.DefaultPass);
                if (chkUser.Succeeded) { await userManager.AddToRoleAsync(usrRapula.Id, admin); }
                chkUser = await userManager.CreateAsync(usrBatman, Properties.SharedResources.DefaultPass);
                if (chkUser.Succeeded) { await userManager.AddToRoleAsync(usrBatman.Id, admin); }
            }

            var manager = Properties.UserRoles.CareSiteInventoryManager;
            if (!await roleManager.RoleExistsAsync(manager))
            {
                await roleManager.CreateAsync(new IdentityRole() { Name = manager });

                var usrLuke = new ApplicationUser()
                {
                    EmailConfirmed = true,
                    Email = "lskywalker@rebels.com",
                    UserName = "notVadersSon"
                };

                var usrJohn = new ApplicationUser()
                {
                    EmailConfirmed = true,
                    Email = "jdoe1@mail.com",
                    UserName = "john-doe"
                };

                var chkUser = await userManager.CreateAsync(usrLuke, Properties.SharedResources.DefaultPass);
                if (chkUser.Succeeded) { await userManager.AddToRoleAsync(usrLuke.Id, manager); }
                chkUser = await userManager.CreateAsync(usrJohn, Properties.SharedResources.DefaultPass);
                if (chkUser.Succeeded) { await userManager.AddToRoleAsync(usrLuke.Id, manager); }
            }

            var employee = Properties.UserRoles.CareSiteStaff;
            if (!await roleManager.RoleExistsAsync(employee))
            {
                await roleManager.CreateAsync(new IdentityRole() { Name = employee });

                var usrBob = new ApplicationUser()
                {
                    EmailConfirmed = true,
                    Email = "bobross@uzima.org",
                    UserName = "rOssB0Bboss"
                };

                var usrChavo = new ApplicationUser()
                {
                    EmailConfirmed = true,
                    Email = "chavo@delocho.net",
                    UserName = "chavo"
                };

                var chkUser = await userManager.CreateAsync(usrBob, Properties.SharedResources.DefaultPass);
                if (chkUser.Succeeded) { await userManager.AddToRoleAsync(usrBob.Id, employee); }
                chkUser = await userManager.CreateAsync(usrChavo, Properties.SharedResources.DefaultPass);
                if (chkUser.Succeeded) { await userManager.AddToRoleAsync(usrBob.Id, employee); }
            }
        }
    }
}
