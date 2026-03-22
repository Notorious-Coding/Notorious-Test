using NotoriousTest.Configuration;

namespace NotoriousTest.Web.Applications
{
    public interface IWebApplication : IAsyncDisposable, IConfigurationConsumer
    {
        HttpClient CreateDefaultClient();
    }
}