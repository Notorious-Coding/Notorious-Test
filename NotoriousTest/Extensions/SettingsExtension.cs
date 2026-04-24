using Microsoft.Extensions.Configuration;

using NotoriousTest.Infrastructures;
using NotoriousTest.Settings;

namespace NotoriousTest.Extensions;

/// <summary>
/// Infrastructure extension that binds a settings section from <c>testsettings.json</c> to a typed settings object.
/// </summary>
/// <typeparam name="TSettings">The settings type to bind. Must have a parameterless constructor.</typeparam>
public class SettingsExtension<TSettings> : IInfrastructureExtension
    where TSettings : class, new()
{
    /// <summary>Gets the bound settings object populated before infrastructure initialization.</summary>
    public TSettings Settings { get; private set; } = new();

    /// <summary>Gets the configuration section name to bind. Defaults to the infrastructure type name when null.</summary>
    protected string? SectionName { get; init; }
    private ITestSettingsProvider _settingsProvider;

    /// <summary>Initializes a new instance with the given settings provider.</summary>
    /// <param name="settingsProvider">The provider used to locate and load test settings.</param>
    public SettingsExtension(ITestSettingsProvider settingsProvider)
    {
        _settingsProvider = settingsProvider ?? throw new ArgumentNullException(nameof(settingsProvider));
    }

    /// <summary>Binds the settings section to <see cref="Settings"/> before the infrastructure initializes.</summary>
    /// <param name="infrastructure">The infrastructure being initialized.</param>
    public Task OnBeforeInitialize(IInfrastructure infrastructure)
    {
        var configuration = _settingsProvider.Find();
        configuration.GetSection(SectionName ?? infrastructure.GetType().Name).Bind(Settings);
        return Task.CompletedTask;
    }
}
