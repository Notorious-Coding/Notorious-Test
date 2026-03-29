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

        public ExternalDatabaseInfrastructure(ContextId contextId, ITestSettingsProvider provider, ITestLogger logger, IRegistry registry) : base(contextId, logger, registry)
        {
            Settings = EnsureExtension(new SettingsExtension<TSettings>(provider)).Settings;
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
