using NotoriousTest.Core;
using NotoriousTest.Core.Logger;
using NotoriousTest.Core.Registry;
using NotoriousTest.Web.Infrastructures;

namespace NotoriousTest.Web.AspNetCore;

/// <inheritdoc />
public class AspNetCoreWebApplicationInfrastructure<TProgram>
    : AspNetCoreWebApplicationInfrastructure<AspNetCoreWebApplication<TProgram>, TProgram> where TProgram : class
{
    /// <inheritdoc />
    public AspNetCoreWebApplicationInfrastructure(EnvironmentId contextId, ITestLogger logger, IRegistry registry) :
        base(contextId, logger, registry)
    {
    }
}

/// <summary>
///     Create an ASP.NET Core web application infrastructure, running <typeparamref name="TWebApp" /> in memory.
/// </summary>
/// <typeparam name="TWebApp">Web application to start, which can override any <c>WebApplicationFactory</c> method.</typeparam>
/// <typeparam name="TEntryPoint">Type of the Program.cs file of the application to start.</typeparam>
public class AspNetCoreWebApplicationInfrastructure<TWebApp, TEntryPoint> : WebApplicationInfrastructure
    where TWebApp : AspNetCoreWebApplication<TEntryPoint>, new()
    where TEntryPoint : class
{
    /// <inheritdoc />
    public AspNetCoreWebApplicationInfrastructure(EnvironmentId contextId, ITestLogger logger, IRegistry registry) :
        base(contextId, logger, registry)
    {
    }

    /// <summary>
    ///     Web application started by the infrastructure.
    /// </summary>
    protected TWebApp WebApplication { get; } = new();

    /// <inheritdoc />
    public override async Task Destroy() => await WebApplication.DisposeAsync();

    /// <inheritdoc />
    public override async Task Initialize()
    {
        WebApplication.ConsumedConfiguration = ConsumedConfiguration;
        foreach ((string key, string value) in AdditionalConfiguration)
            WebApplication.AdditionalConfiguration[key] = value;

        HttpClient = await WebApplication.Start();
    }
}
