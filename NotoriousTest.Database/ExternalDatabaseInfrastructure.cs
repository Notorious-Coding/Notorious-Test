using NotoriousTest.Database.Settings;
using NotoriousTest.Extensions;

namespace NotoriousTest.Database
{
    public abstract class ExternalDatabaseInfrastructure<TOutputConfiguration, TSettings> : DatabaseInfrastructureBase<TOutputConfiguration> where TSettings : DatabaseSettings, new()
    {
        protected TSettings Settings { get; private set; }

        public ExternalDatabaseInfrastructure() : base()
        {
            Settings = EnsureExtension(new SettingsExtension<TSettings>()).Settings;
        }

        public override string GetServerConnectionString()
        {
            return Settings.ConnectionString;
        }
    }
}
