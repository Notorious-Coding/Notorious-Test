using NotoriousTest.Core.Environments;

namespace NotoriousTest.Web.Playwright;

public static class EnvironmentExtensions
{
    extension(EnvironmentBase environment)
    {
        public EnvironmentBase AddPlaywright() => environment.AddInfrastructure<BrowserInfrastructure>();
    }
}
