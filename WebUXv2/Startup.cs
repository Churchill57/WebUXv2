using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(WebUXv2.Startup))]
namespace WebUXv2
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
