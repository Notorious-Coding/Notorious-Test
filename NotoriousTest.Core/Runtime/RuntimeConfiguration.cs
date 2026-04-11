namespace NotoriousTest.Core.Runtime
{
    public record RuntimeConfiguration(string tfm, SupportedFramework[]? SupportedFrameworks);

    public record SupportedFramework(string Name, string Version, string FilePath)
    {
    };
}
