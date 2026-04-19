using NotoriousTest.Core.Infrastructures;
using NotoriousTest.Core.Settings;

namespace NotoriousTest.Core.Extensions;

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
        Settings = _settingsProvider.Get<TSettings>(SectionName ?? infrastructure.GetType().Name) ?? throw new InfrastructureSettingsNotFound($"Settings for {typeof(TSettings).Name} not found. Verify that the section '{SectionName ?? infrastructure.GetType().Name}' exists in the configuration.");

        return Task.CompletedTask;
    }
}
