using System.Reflection;
using Xunit.Sdk;
using Xunit.v3;

namespace NotoriousTest.IntegrationTests.Ordering;

/// <summary>
///     Orders test methods by their <see cref="OrderAttribute" /> value (ascending).
///     Tests without the attribute run last, in alphabetical order.
/// </summary>
public class OrderAttributeTestMethodOrderer : ITestMethodOrderer
{
    public IReadOnlyCollection<TTestMethod?> OrderTestMethods<TTestMethod>(IReadOnlyCollection<TTestMethod?> testMethods)
        where TTestMethod : notnull, ITestMethod =>
        testMethods
            .OrderBy(testMethod => GetOrder(testMethod))
            .ThenBy(testMethod => testMethod?.MethodName, StringComparer.Ordinal)
            .ToList();

    private static int GetOrder(ITestMethod? testMethod)
    {
        if (testMethod is not IXunitTestMethod xunitTestMethod)
            return int.MaxValue;

        return xunitTestMethod.Method.GetCustomAttribute<OrderAttribute>()?.Order ?? int.MaxValue;
    }
}
