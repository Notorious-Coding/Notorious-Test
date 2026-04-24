using NotoriousTest.Configuration;

namespace NotoriousTest.Web.Applications
{
    /// <summary>
    /// Represents a test web application factory that supports configuration injection and HTTP client creation.
    /// </summary>
    public interface IWebApplication : IAsyncDisposable, IConfigurationConsumer
    {
        /// <summary>Creates and returns a default <see cref="HttpClient"/> for the test web application.</summary>
        HttpClient CreateDefaultClient();
    }
}
