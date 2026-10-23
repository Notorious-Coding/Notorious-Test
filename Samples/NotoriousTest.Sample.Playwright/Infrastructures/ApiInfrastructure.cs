using NotoriousTest.Core;
using NotoriousTest.Core.Logger;
using NotoriousTest.Core.Registry;
using NotoriousTest.Web.AspNetCore;

namespace NotoriousTest.Sample.Playwright.Infrastructures;

/// <summary>
///     NotoriousTest.Sample.TestWebApp, started with its WebApplicationFactory.
///     Unlike the in-memory TestServer, the API listens on a real port with Kestrel,
///     so the front (and therefore the browser) can call it.
/// </summary>
public class
    ApiInfrastructure : AspNetCoreWebApplicationInfrastructure<KestrelAspNetCoreWebApplication<Program>, Program>
{
    // Started before the front, which needs the API url.
    public override int? Order => 998;

    public ApiInfrastructure(EnvironmentId contextId, ITestLogger logger, IRegistry registry) : base(contextId,
        logger, registry)
    {
    }
}
