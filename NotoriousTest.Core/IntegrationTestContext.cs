using System.Reflection;

namespace NotoriousTest.Core;

public class IntegrationTestContext
{
    public static IntegrationTestContext Current
    {
        get => field ??= new IntegrationTestContext();
    }

    public Assembly CurrentAssembly { get; set; }

}
