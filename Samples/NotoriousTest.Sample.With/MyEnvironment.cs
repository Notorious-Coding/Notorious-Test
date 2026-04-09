using NotoriousTest.Web;

using System.Reflection;

using Xunit.Sdk;

namespace NotoriousTest.Sample.With
{
    public class MyEnvironment : XUnit.Environment
    {
        public MyEnvironment(IMessageSink sink) : base(sink)
        {
        }

        public override Assembly CurrentAssembly => Assembly.GetExecutingAssembly();

        public override async Task ConfigureEnvironment()
        {
            AddInfrastructure<MySqlServerInfrastructure>();
            this.AddWebApplication<MyWebApplication>();
        }
    }
}
