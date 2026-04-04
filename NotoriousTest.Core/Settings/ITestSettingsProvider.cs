using Microsoft.Extensions.Configuration;

namespace NotoriousTest.Core.Settings
{
    public interface ITestSettingsProvider
    {
        IConfiguration Find();
    }
}