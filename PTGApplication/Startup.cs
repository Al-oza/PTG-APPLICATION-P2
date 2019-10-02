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

            if (!roleManager.RoleExists("Admin"))
            {
                await roleManager.CreateAsync(new IdentityRole() { Name = "Admin" });

                var usrRapula = new ApplicationUser()
                {
                    EmailConfirmed = true,
                    Email = Properties.SharedResources.DefaultEmail,
                    UserName = Properties.SharedResources.DefaultUser
                };
                var chkUser = await userManager.CreateAsync(usrRapula, Properties.SharedResources.DefaultPass);
                if (chkUser.Succeeded) { await userManager.AddToRoleAsync(usrRapula.Id, "Admin"); }
            }

            if (!await roleManager.RoleExistsAsync("Manager"))
            { await roleManager.CreateAsync(new IdentityRole() { Name = "Manager" }); }

            if (!await roleManager.RoleExistsAsync("Employee"))
            {
                await roleManager.CreateAsync(new IdentityRole() { Name = "Employee" });

                var usrBob = new ApplicationUser()
                {
                    EmailConfirmed = true,
                    Email = "bobhope@uzima.org",
                    UserName = "bob"
                };
                var chkUser = await userManager.CreateAsync(usrBob, Properties.SharedResources.DefaultPass);
                if (chkUser.Succeeded) { await userManager.AddToRoleAsync(usrBob.Id, "Employee"); }
            }
        }
    }
}
