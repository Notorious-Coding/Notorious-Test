namespace NotoriousTest.Core
{
    public record ContextId(Guid Value)
    {
        public static implicit operator Guid(ContextId wrapper) => wrapper.Value;
        public static implicit operator ContextId(Guid guid) => new(guid);
    }
}
