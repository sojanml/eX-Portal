using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(eX_Portal.Startup))]
namespace eX_Portal
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
