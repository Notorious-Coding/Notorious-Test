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
        var configuration = _settingsProvider.Get<TSettings>(SectionName ?? infrastructure.GetType().Name);
        return Task.CompletedTask;
    }
}