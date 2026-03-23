using NotoriousTest.Database.Settings;
using NotoriousTest.Extensions;
using NotoriousTest.Logger;
using NotoriousTest.Settings;

namespace NotoriousTest.Database
{
    public abstract class ExternalDatabaseInfrastructure<TOutputConfiguration, TSettings> : DatabaseInfrastructureBase<TOutputConfiguration> where TSettings : DatabaseSettings, new()
    {
        protected TSettings Settings { get; private set; }

        public ExternalDatabaseInfrastructure(ContextId contextId, ITestSettingsProvider provider, ITestLogger logger) : base(contextId, logger)
        {
            Settings = EnsureExtension(new SettingsExtension<TSettings>(provider)).Settings;
        }

        public override string GetServerConnectionString()
        {
            return Settings.ConnectionString;
        }
    }
}
