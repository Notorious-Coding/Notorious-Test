namespace NotoriousTest.Core.DI;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class InjectionConfiguratorAttribute(Type diConfiguratorType) : Attribute
{
    public Type DIConfiguratorType { get; } = diConfiguratorType;
}
