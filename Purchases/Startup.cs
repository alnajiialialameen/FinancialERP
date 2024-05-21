using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Purchases.Startup))]
namespace Purchases
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
