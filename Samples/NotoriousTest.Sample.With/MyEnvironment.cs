using NotoriousTest.Web;
using System.Reflection;
using Xunit.Sdk;
using Environment = NotoriousTest.XUnit.Environment;

namespace NotoriousTest.Sample.With;

public class MyEnvironment : Environment
{
    public MyEnvironment(IMessageSink sink) : base(sink)
    {
    }

    public override Assembly CurrentAssembly => Assembly.GetExecutingAssembly();

    public override async Task ConfigureEnvironment()
    {
        AddInfrastructure<MySqlServerInfrastructure>();
        this.AddWebApplication<MyAspNetCoreWebApplication>();
    }
}
