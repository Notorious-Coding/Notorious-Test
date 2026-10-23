using NotoriousTest.Core;
using NotoriousTest.Core.Configuration;
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
    protected override string ProjectDir { get; } = FindFrontDirectory();

    protected override string Command => "dev";


    public FrontInfrastructure(EnvironmentId contextId, ITestLogger logger, IRegistry registry) : base(contextId,
        logger, registry)
    {
        Dependencies.Add(new NpmInstallDependency(ProjectDir));
    }

    protected override Task<Dictionary<string, string>> ConfigureApplication(
        List<ConfigurationEntry<object>> configurationFromOtherInfrastructures) =>
        Task.FromResult(new Dictionary<string, string>
        {
            {
                "API_URL", configurationFromOtherInfrastructures.WebApiUrl ??
                           throw new InvalidOperationException("WebApiUrl not found.")
            }
        });


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
