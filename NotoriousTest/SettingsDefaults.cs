namespace NotoriousTest
{
    internal static class SettingsDefaults
    {
        public static readonly string DefaultRegistryConnectionString = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "notorioustest/doggydog-registry.db");
    }
}
