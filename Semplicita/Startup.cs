using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Semplicita.Startup))]
namespace Semplicita
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
