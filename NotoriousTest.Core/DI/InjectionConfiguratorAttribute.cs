namespace NotoriousTest.Core.DI;

/// <summary>
///     Apply the attribute on a class inheriting from <cref>IntegrationTestBase</cref>  to configure the DI Container
///     associated.
/// </summary>
/// <param name="diConfiguratorType">Type of dependency configurator.</param>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class InjectionConfiguratorAttribute(Type diConfiguratorType) : Attribute
{
    /// <summary>
    ///     Type of dependency configurator.
    /// </summary>
    public Type DIConfiguratorType { get; } = diConfiguratorType;
}
