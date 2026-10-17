using NotoriousTest.Core;
using NotoriousTest.Core.Logger;
using NotoriousTest.Core.Registry;
using NotoriousTest.Sample.Playwright.Dependencies;
using NotoriousTest.Web;
using NotoriousTest.Web.Npm;

namespace NotoriousTest.Sample.Playwright.Infrastructures;

/// <summary>
///     React application (NotoriousTest.Sample.Front), started with the Vite dev server (<c>npm run dev</c>).
/// </summary>
public class FrontInfrastructure : NpmWebApplicationInfrastructure
{
    public FrontInfrastructure(EnvironmentId contextId, ITestLogger logger, IRegistry registry) : base(contextId,
        logger, registry)
    {
        Dependencies.Add(new NpmInstallDependency(ProjectDir));
    }

    protected override string ProjectDir { get; } = FindFrontDirectory();

    protected override string Command => "dev";

    public override async Task Initialize()
    {
        await base.Initialize();

        // Url opened by the BrowserInfrastructure.
        OutputConfiguration.WebAppUrl = HttpClient!.BaseAddress!.ToString();
    }

    protected override Task ConfigureApplication(Dictionary<string, string?> configuration,
        IDictionary<string, string?> environmentVariables)
    {
        // Read by vite.config.js to proxy /api/* to the API started by ApiInfrastructure.
        environmentVariables["API_URL"] = configuration[ApiInfrastructure.API_URL_KEY];
        return Task.CompletedTask;
    }

    private static string FindFrontDirectory()
    {
        const string frontDirectoryName = "NotoriousTest.Sample.Front";

        for (DirectoryInfo? directory = new(AppContext.BaseDirectory); directory != null; directory = directory.Parent)
        {
            string candidate = Path.Combine(directory.FullName, frontDirectoryName);
            if (Directory.Exists(candidate)) return candidate;
        }

        throw new DirectoryNotFoundException($"{frontDirectoryName} not found from {AppContext.BaseDirectory}.");
    }
}
