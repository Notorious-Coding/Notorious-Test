using NotoriousTest.Core;
using NotoriousTest.Core.Environments;
using NotoriousTest.Core.Infrastructures;
using NotoriousTest.Core.Logger;
using NotoriousTest.Core.Registry;
using NotoriousTest.Core.Settings;
using NotoriousTest.Database.Settings;

namespace NotoriousTest.Database
{
    public abstract class ExternalDatabaseInfrastructure<TOutputConfiguration, TSettings> : DatabaseInfrastructureBase<TOutputConfiguration, DatabaseMetadata> where TSettings : DatabaseSettings, new()
    {
        protected TSettings Settings { get; private set; }
        protected string? SectionName { get; set; } = null;

        public ExternalDatabaseInfrastructure(EnvironmentId contextId, ITestSettingsProvider provider, ITestLogger logger, IRegistry registry, EnvironmentSettings settings) : base(contextId, logger, registry, settings)
        {
            Settings = provider.Get<TSettings>(SectionName ?? GetType().Name) ?? throw new InfrastructureSettingsNotFound($"Settings in section {SectionName ?? this.GetType().Name} not found.");
        }

        public override string GetServerConnectionString() => Settings.ConnectionString;

        public override async Task Initialize()
        {
            Metadata = new DatabaseMetadata()
            {
                DatabaseName = FullDbName,
                ServerConnectionString = GetServerConnectionString()
            };
            await Register();
            await base.Initialize();
        }
    }
}
