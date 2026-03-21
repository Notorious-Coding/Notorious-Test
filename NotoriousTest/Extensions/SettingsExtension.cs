using Microsoft.Extensions.Configuration;

using NotoriousTest.Infrastructures;
using NotoriousTest.Settings;

namespace NotoriousTest.Extensions;

public class SettingsExtension<TSettings> : IInfrastructureExtension
    where TSettings : class, new()
{
    public TSettings Settings { get; private set; } = new();

    protected string? SectionName { get; init; }

    public Task OnBeforeInitialize(IInfrastructure infrastructure)
    {
        var configuration = TestSettingsProvider.Find();
        configuration.GetSection(SectionName ?? infrastructure.GetType().Name).Bind(Settings);
        return Task.CompletedTask;
    }
}