namespace NotoriousTest.Environments
{
    internal abstract class SqliteRegistryConfigurationDefaults
    {
        public static readonly string DefaultRegistryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "notorioustest/doggydog-registry.db");
    }
}
