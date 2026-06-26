namespace NotoriousTest.Internal.Runtime;

internal record RuntimeConfigurationEntity(RuntimeOptions RuntimeOptions);

internal record RuntimeOptions(
    string Tfm,
    FrameworkEntry? Framework,
    FrameworkEntry[]? Frameworks
);

internal record FrameworkEntry(string Name, string Version);
