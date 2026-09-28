namespace NotoriousTest.IntegrationTests.Ordering;

/// <summary>
///     Defines the execution order of a test inside its class.
///     Requires the class to be decorated with <c>[TestMethodOrderer(typeof(OrderAttributeTestMethodOrderer))]</c>.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class OrderAttribute(int order) : Attribute
{
    public int Order { get; } = order;
}
