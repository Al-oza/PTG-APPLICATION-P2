using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin;
using Owin;
using PTGApplication.Models;
using System;
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
            ConfigureDatabase();
            ConfigureAuth(app);
            CreateRolesAndUsers().Wait();
        }

        #region Configure Database and Default Users
        private async Task ConfigureAdmins(String admin, UserManager<ApplicationUser> userManager)
        {
            var user = new ApplicationUser()
            {
                EmailConfirmed = true,
                Email = "rapula@uzima.org",
                HomePharmacy = "warehouse1",
                Name = "Rapula Otukile",
                UserName = "rapula",
                IsActive = true
            };

            var chkUser = await userManager.CreateAsync(user, Properties.SharedResources.DefaultPass);
            if (chkUser.Succeeded) { userManager.AddToRole(user.Id, admin); }

            user = new ApplicationUser()
            {
                EmailConfirmed = true,
                Email = "brucewayne@gothem.net",
                HomePharmacy = "clinic",
                Name = "Bruce Wayne",
                UserName = "batman",
                IsActive = true
            };

            chkUser = await userManager.CreateAsync(user, Properties.SharedResources.DefaultPass);
            if (chkUser.Succeeded) { userManager.AddToRole(user.Id, admin); }
        }
        private void ConfigureDatabase()
        {
            var configuration = WebConfigurationManager.OpenWebConfiguration("~");
            var section = (ConnectionStringsSection)configuration.GetSection("connectionStrings");


            configuration.Save();
        }
        private async Task ConfigureEmployees(String employee, UserManager<ApplicationUser> userManager)
        {
            var user = new ApplicationUser()
            {
                EmailConfirmed = true,
                Email = "bobross@uzima.org",
                HomePharmacy = "clinic",
                Name = "Bob Ross",
                UserName = "rOssBoBbOss",
                IsActive = true
            };

            var chkUser = await userManager.CreateAsync(user, Properties.SharedResources.DefaultPass);
            if (chkUser.Succeeded) { await userManager.AddToRoleAsync(user.Id, employee); }

            user = new ApplicationUser()
            {
                EmailConfirmed = true,
                Email = "joey@hotail.com",
                HomePharmacy = "hospital",
                Name = "Joey Tribiani",
                UserName = "joeyt",
                IsActive = true
            };

            chkUser = await userManager.CreateAsync(user, Properties.SharedResources.DefaultPass);
            if (chkUser.Succeeded) { await userManager.AddToRoleAsync(user.Id, employee); }
        }
        private async Task ConfigureManagers(String manager, UserManager<ApplicationUser> userManager)
        {
            var user = new ApplicationUser()
            {
                EmailConfirmed = true,
                Email = "lskywalker@rebels.com",
                HomePharmacy = "clinic",
                Name = "Luke Skywalker",
                UserName = "notVadersSon",
                IsActive = true
            };

            var chkUser = await userManager.CreateAsync(user, Properties.SharedResources.DefaultPass);
            if (chkUser.Succeeded) { await userManager.AddToRoleAsync(user.Id, manager); }

            user = new ApplicationUser()
            {
                EmailConfirmed = true,
                Email = "jdoe1@mail.com",
                HomePharmacy = "hospital",
                Name = "John Doe",
                UserName = "johndoe",
                IsActive = true
            };

            chkUser = await userManager.CreateAsync(user, Properties.SharedResources.DefaultPass);
            if (chkUser.Succeeded) { await userManager.AddToRoleAsync(user.Id, manager); }
        }
        private async Task CreateRolesAndUsers()
        {
            using (var context = new ApplicationDbContext())
            using (var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context)))
            using (var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context)))
            {
                var admin = Properties.UserRoles.PharmacyManager;
                if (!roleManager.RoleExists(admin))
                {
                    await roleManager.CreateAsync(new IdentityRole() { Name = admin });
                    await ConfigureAdmins(admin, userManager);
                }

                var manager = Properties.UserRoles.CareSiteInventoryManager;
                if (!await roleManager.RoleExistsAsync(manager))
                {
                    await roleManager.CreateAsync(new IdentityRole() { Name = manager });
                    await ConfigureManagers(manager, userManager);
                }

                var employee = Properties.UserRoles.CareSiteStaff;
                if (!await roleManager.RoleExistsAsync(employee))
                {
                    await roleManager.CreateAsync(new IdentityRole() { Name = employee });
                    await ConfigureEmployees(employee, userManager);
                }

            }
        }
        #endregion
    }
}
