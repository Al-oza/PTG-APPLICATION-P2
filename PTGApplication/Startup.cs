﻿using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(PTGApplication.Startup))]

namespace PTGApplication
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
