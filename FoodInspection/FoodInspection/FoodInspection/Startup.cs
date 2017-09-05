using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(FoodInspection.Startup))]
namespace FoodInspection
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
