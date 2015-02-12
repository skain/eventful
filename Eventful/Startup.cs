using Eventful.Code;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Eventful.Startup))]
namespace Eventful
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
			ConfigureAuth(app);
			AutoMapperConfiguration.ConfigureMappings();
        }
    }
}
