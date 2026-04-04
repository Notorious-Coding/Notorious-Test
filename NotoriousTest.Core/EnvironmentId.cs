namespace NotoriousTest.Core
{
    public record EnvironmentId(Guid Value)
    {
        public static implicit operator Guid(EnvironmentId wrapper) => wrapper.Value;
        public static implicit operator EnvironmentId(Guid guid) => new(guid);
    }
}
