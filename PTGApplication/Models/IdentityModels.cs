using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace PTGApplication.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit https://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        public string HomePharmacy { get; set; }
        public bool IsActive { get; set; }
        public string Name { get; set; }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("Data Source=uzimarx.database.windows.net;Initial Catalog=uzimarx;Persist Security Info=True;User ID=uzimarx-app;Password=tyqh34@$sdlfia3dlkfPOJ23LKUwnudfn*(t789qe*4",
                  throwIfV1Schema: false)
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        public System.Data.Entity.DbSet<PTGApplication.Models.UzimaLocation> UzimaLocations { get; set; }

        public System.Data.Entity.DbSet<PTGApplication.Models.UzimaInventory> UzimaInventories { get; set; }

        public System.Data.Entity.DbSet<PTGApplication.Models.AspNetUser> AspNetUsers { get; set; }
    }
}