using Masaking.Infrastructure;
using Microsoft.Owin;
using Microsoft.Owin.Security.DataProtection;
using Owin;

[assembly: OwinStartup(typeof(Masaking.Startup))]

namespace Masaking
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // Use the System.Web host's application-scoped protection provider.
            OwinRecoveryTokens.Configure(app.GetDataProtectionProvider());
        }
    }
}
