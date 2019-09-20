using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Configuration;
using Microsoft.Owin;
using Owin;

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
        }
    }
}
