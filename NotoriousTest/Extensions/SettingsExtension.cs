using Microsoft.Extensions.Configuration;

using NotoriousTest.Infrastructures;
using NotoriousTest.Settings;

namespace NotoriousTest.Extensions;

public class SettingsExtension<TSettings> : IInfrastructureExtension
    where TSettings : class, new()
{
    public TSettings Settings { get; private set; } = new();

    protected string? SectionName { get; init; }
    private ITestSettingsProvider _settingsProvider;

    public SettingsExtension(ITestSettingsProvider settingsProvider)
    {
        _settingsProvider = settingsProvider ?? throw new ArgumentNullException(nameof(settingsProvider));
    }

    public Task OnBeforeInitialize(IInfrastructure infrastructure)
    {
        var configuration = _settingsProvider.Find();
        configuration.GetSection(SectionName ?? infrastructure.GetType().Name).Bind(Settings);
        return Task.CompletedTask;
    }
}