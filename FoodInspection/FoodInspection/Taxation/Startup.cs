using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Taxation.Startup))]
namespace Taxation
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
           
        }
    }
}
