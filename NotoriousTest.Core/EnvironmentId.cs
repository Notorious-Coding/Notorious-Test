namespace NotoriousTest.Core;

/// <summary>
///     Identifier for an environment.
/// </summary>
/// <param name="Value">Guid.</param>
public record EnvironmentId(Guid Value)
{
    public static implicit operator Guid(EnvironmentId wrapper)
    {
        return wrapper.Value;
    }

    public static implicit operator EnvironmentId(Guid guid)
    {
        return new EnvironmentId(guid);
    }


    /// <summary>
    ///     Creates a new environment identifier.
    /// </summary>
    /// <returns>An environment identifier.</returns>
    public static EnvironmentId Create() => new(Guid.NewGuid());
}
