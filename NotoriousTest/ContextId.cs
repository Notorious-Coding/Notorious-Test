namespace NotoriousTest
{
    /// <summary>
    /// A strongly-typed wrapper around <see cref="Guid"/> used to uniquely identify a test context.
    /// </summary>
    /// <param name="Value">The underlying <see cref="Guid"/> value.</param>
    public record ContextId(Guid Value)
    {
        /// <summary>Implicitly converts a <see cref="ContextId"/> to its underlying <see cref="Guid"/>.</summary>
        public static implicit operator Guid(ContextId wrapper) => wrapper.Value;

        /// <summary>Implicitly converts a <see cref="Guid"/> to a <see cref="ContextId"/>.</summary>
        public static implicit operator ContextId(Guid guid) => new(guid);
    }
}
