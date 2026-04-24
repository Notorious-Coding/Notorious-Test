using Microsoft.Extensions.Configuration;

namespace NotoriousTest.Settings
{
    /// <summary>
    /// Provides access to test settings configuration.
    /// </summary>
    public interface ITestSettingsProvider
    {
        /// <summary>Finds and returns the test settings configuration.</summary>
        IConfiguration Find();
    }
}
