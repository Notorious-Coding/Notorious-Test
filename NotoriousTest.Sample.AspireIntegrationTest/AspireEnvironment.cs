using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using NotoriousTest.Web;
using NotoriousTest.Web.Applications;

using System.Reflection;

using Xunit.Sdk;

namespace NotoriousTest.Sample.AspireIntegrationTest
{
    public class AspireEnvironment : XUnit.Environment
    {
        public override Assembly CurrentAssembly => Assembly.GetExecutingAssembly();
        protected IDistributedApplicationBuilder DistributedApplicationBuilder { get; private set; }
        protected IHost Host { get; private set; }
        public AspireEnvironment(IMessageSink sink) : base(sink)
        {
            var options = new DistributedApplicationOptions { AssemblyName = CurrentAssembly.FullName, DisableDashboard = true };
            DistributedApplicationBuilder = DistributedApplication.CreateBuilder(options);
        }


        public override async Task ConfigureEnvironment()
        {
            AddInfrastructure<SqlServerAspireInfrastructure>();
            this.AddWebApplication<WebApplication<Program>>();

            Host = DistributedApplicationBuilder.Build();
            await Host.StartAsync();
        }

        public override void ConfigureInfrastructureServices(IServiceCollection collection)
        {
            base.ConfigureInfrastructureServices(collection);

            collection.AddSingleton(DistributedApplicationBuilder);
        }

        public override Task Initialize()
        {
            return base.Initialize();
        }

        public override async Task Destroy()
        {
            await Host.StopAsync();
            await base.Destroy();
        }
    }
}
