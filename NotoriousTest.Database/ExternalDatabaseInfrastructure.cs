using NotoriousTest.Core;
using NotoriousTest.Core.Extensions;
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

        public ExternalDatabaseInfrastructure(EnvironmentId contextId, ITestSettingsProvider provider, ITestLogger logger, IRegistry registry) : base(contextId, logger, registry)
        {
            Settings = provider.Get<TSettings>(SectionName ?? this.GetType().Name) ?? throw new InfrastructureSettingsNotFound($"Settings in section {SectionName ?? this.GetType().Name} not found.");
        }

        public override string GetServerConnectionString()
        {
            return Settings.ConnectionString;
        }

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
