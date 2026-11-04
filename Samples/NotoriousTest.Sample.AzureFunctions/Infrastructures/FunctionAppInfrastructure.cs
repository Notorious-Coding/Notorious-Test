using NotoriousTest.Core;
using NotoriousTest.Core.Logger;
using NotoriousTest.Core.Registry;
using NotoriousTest.Web.AzureFunctions;

namespace NotoriousTest.Sample.AzureFunctions.Infrastructures;

/// <summary>
///     NotoriousTest.Sample.FunctionApp, started locally with <c>func host start</c> on a free port.
///     It receives the configuration of the previous infrastructures as environment variables
///     (<c>ConnectionStrings:SqlServer</c> becomes <c>ConnectionStrings__SqlServer</c>).
///     The Azure Functions Core Tools (func) are installed by the environment if missing.
/// </summary>
public class FunctionAppInfrastructure : AzureFunctionWebApplicationInfrastructure
{
    public FunctionAppInfrastructure(EnvironmentId contextId, ITestLogger logger, IRegistry registry) : base(contextId,
        logger, registry)
    {
    }

    // From bin/Debug/net10.0 up to the Samples directory.
    protected override string FunctionProjectDir { get; } = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "../../../../NotoriousTest.Sample.FunctionApp"));
}
