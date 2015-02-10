using eventful.Code;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(eventful.Startup))]
namespace eventful
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
