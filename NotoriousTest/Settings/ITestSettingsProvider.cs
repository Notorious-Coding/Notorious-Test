using Microsoft.Extensions.Configuration;

namespace NotoriousTest.Settings
{
    public interface ITestSettingsProvider
    {
        IConfiguration Find();
    }
}