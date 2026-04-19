using Microsoft.Extensions.Configuration;

namespace NotoriousTest.Core.Settings
{
    public interface ITestSettingsProvider
    {
        IConfiguration Find();
        T? Get<T>(string key) where T : new();
    }
}