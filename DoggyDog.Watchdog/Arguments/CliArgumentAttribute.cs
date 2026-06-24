namespace DoggyDog.Watchdog.Arguments;

[AttributeUsage(AttributeTargets.Property)]
internal class CliArgumentAttribute(string Name, bool Required = true) : Attribute
{
    public string Name { get; } = Name;
    public bool Required { get; } = Required;
}
