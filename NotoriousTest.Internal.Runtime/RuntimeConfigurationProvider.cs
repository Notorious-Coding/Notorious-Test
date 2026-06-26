using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using NotoriousTest.Core.Logger;
using NotoriousTest.Core.Runtime;

namespace NotoriousTest.Internal.Runtime;

internal class RuntimeConfigurationProvider : IRuntime
{
    private const string RuntimeFile = "runtimeconfig.json";
    private readonly ITestLogger _logger;

    private readonly JsonSerializerOptions JSON_OPTIONS = new() { PropertyNameCaseInsensitive = true };
    private RuntimeConfiguration? _cache;

    public RuntimeConfigurationProvider(ITestLogger logger)
    {
        _logger = logger;
    }

    public RuntimeConfiguration? GetSupportedRuntimes(Assembly executingAssembly)
    {
        if (_cache != null) return _cache;

        string assemblyPath = executingAssembly.Location;
        string runtimePath = Path.ChangeExtension(assemblyPath, RuntimeFile);

        if (!File.Exists(runtimePath))
            return null;


        string json = File.ReadAllText(runtimePath);


        RuntimeConfigurationEntity? entity = JsonSerializer.Deserialize<RuntimeConfigurationEntity>(json, JSON_OPTIONS);
        if (entity == null)
            return null;

        FrameworkEntry[]? frameworkEntries = entity.RuntimeOptions.Frameworks ?? [entity.RuntimeOptions.Framework!];
        SupportedFramework[]? frameworks = frameworkEntries.Select(ToDomain).ToArray();

        var result = new RuntimeConfiguration(entity.RuntimeOptions.Tfm, frameworks);
        _cache = result;
        return result;
    }

    private SupportedFramework ToDomain(FrameworkEntry entry) =>
        new(entry.Name, entry.Version, GetNearestVersionDirectory(entry));

    private string? GetNearestVersionDirectory(FrameworkEntry framework)
    {
        string dotnetRoot = GetDotnetRoot();

        string frameworkPath = $"{dotnetRoot}/shared/{framework.Name}";

        string prefix = $"{new Version(framework.Version).Major}.{new Version(framework.Version).Minor}.";
        // Find the nearest version
        string? best = Directory.GetDirectories(frameworkPath)
            .Where(d => Path.GetFileName(d).StartsWith(prefix))
            .OrderByDescending(d => new Version(Path.GetFileName(d)))
            .FirstOrDefault();

        return best;
    }

    private string GetDotnetRoot()
    {
        string? dotnetRoot = Environment.GetEnvironmentVariable("DOTNET_ROOT");

        if (!string.IsNullOrWhiteSpace(dotnetRoot))
            return dotnetRoot;

        string mainModule = Process.GetCurrentProcess().MainModule!.FileName;

        // If the test project is self-contained (wich is rare), the main module will be the test executable itself.
        bool isNotSelfContained = Path.GetFileNameWithoutExtension(mainModule)
            .Equals("dotnet", StringComparison.OrdinalIgnoreCase);

        if (isNotSelfContained)
            return Path.GetDirectoryName(mainModule)!;

        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
            "dotnet"
        );
    }
}
