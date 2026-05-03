using NotoriousTest.Core.Environments;
using NotoriousTest.Web.Applications;
using NotoriousTest.Web.Infrastructures;


namespace NotoriousTest.Web
{
    public static class WebEnvironmentExtensions
    {
        public static EnvironmentBase AddWebApplication<TWebApp>(this EnvironmentBase environment)
            where TWebApp : IWebApplication, new()
        {
            environment.AddInfrastructure<WebApplicationInfrastructure<TWebApp>>();
            return environment;
        }

        public static WebApplicationInfrastructure GetWebApplication(this EnvironmentBase environment) => environment.GetInfrastructure<WebApplicationInfrastructure>();

    }
}
