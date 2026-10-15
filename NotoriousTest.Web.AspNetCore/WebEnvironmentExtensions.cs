using NotoriousTest.Core.Environments;

namespace NotoriousTest.Web.AspNetCore;

public static class WebEnvironmentExtensions
{
    /// <summary>
    ///     Add a web application from a Program.cs file to the environment.
    /// </summary>
    /// <param name="environment">Environment.</param>
    /// <typeparam name="TProgram">Program.cs file type</typeparam>
    /// <returns>Environment configured.</returns>
    public static EnvironmentBase AddAspNetWebApplication<TProgram>(this EnvironmentBase environment)
        where TProgram : class =>
        environment.AddInfrastructure<AspNetCoreWebApplicationInfrastructure<TProgram>>();
}
