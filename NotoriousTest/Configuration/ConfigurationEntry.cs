namespace NotoriousTest.Configuration
{
    public record ConfigurationEntry<T>
    {
        public T Value { get; set; }
        public string Key { get; set; }

        public ConfigurationEntry(T value, string? section = null)
        {
            Value = value;
            Key = section ?? value.GetType().Name ?? throw new ArgumentNullException(nameof(value));
        }
    };
}

