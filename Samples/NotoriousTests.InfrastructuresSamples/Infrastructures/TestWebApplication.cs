
using NotoriousTest.Web.Applications;
namespace NotoriousTests.InfrastructuresSamples.Infrastructures
{
    /// <summary>
    /// A web application will automatically consume configuration from the environment ! No more code needed. 
    /// But you can still override WebApplicationFactory methods if you need to.
    /// </summary>
    public class TestWebApplication : WebApplication<Program>
    {

    }
}
