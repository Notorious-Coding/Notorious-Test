using NotoriousTest.Core.Environments;

namespace NotoriousTest.Web.AspNetCore;

public static class WebEnvironmentExtensions
{
    /// <summary>
    ///     Add a web application from a Program.cs file to the current group of infrastructures.
    /// </summary>
    /// <param name="builder">Infrastructure list builder.</param>
    /// <typeparam name="TProgram">Program.cs file type</typeparam>
    /// <returns>Builder configured.</returns>
    public static InfrastructureListBuilder AddAspNetWebApplication<TProgram>(this InfrastructureListBuilder builder)
        where TProgram : class =>
        builder.Add<AspNetCoreWebApplicationInfrastructure<TProgram>>();

    /// <summary>
    ///     Add a web application from a Program.cs file in a new group, started after the previous ones.
    /// </summary>
    /// <param name="builder">Infrastructure list builder.</param>
    /// <typeparam name="TProgram">Program.cs file type</typeparam>
    /// <returns>Builder configured.</returns>
    public static InfrastructureListBuilder ThenAddAspNetWebApplication<TProgram>(
        this InfrastructureListBuilder builder)
        where TProgram : class =>
        builder.ThenAdd<AspNetCoreWebApplicationInfrastructure<TProgram>>();
}
