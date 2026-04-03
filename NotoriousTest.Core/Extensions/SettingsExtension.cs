using Microsoft.Extensions.Configuration;

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
        var configuration = _settingsProvider.Find();
        var section = configuration.GetSection(SectionName ?? infrastructure.GetType().Name);

        if (!section.Exists())
        {
            throw new SectionNotFoundException($"La section {SectionName ?? infrastructure.GetType().Name} n'a pas été trouvé dans le testsettings.json");
        }
        section.Bind(Settings);

        return Task.CompletedTask;
    }
}