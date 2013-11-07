using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(HaemophilusWeb.Startup))]
namespace HaemophilusWeb
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
