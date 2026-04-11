namespace NotoriousTest.Runtime
{
    public record RuntimeConfigurationEntity(RuntimeOptions RuntimeOptions);

    public record RuntimeOptions(
        string Tfm,
        FrameworkEntry? Framework,
        FrameworkEntry[]? Frameworks
    );

    public record FrameworkEntry(string Name, string Version);
}
