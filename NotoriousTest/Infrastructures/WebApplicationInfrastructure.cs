using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotoriousTest.Infrastructures
{
    public abstract class WebApplicationInfrastructure<TEntryPoint> : Infrastructure where TEntryPoint : class
    {
        private readonly WebApplicationFactory<TEntryPoint> _webApplicationFactory;
        public HttpClient HttpClient;
        public WebApplicationInfrastructure(WebApplicationFactory<TEntryPoint> webApplicationFactory)
        {
            _webApplicationFactory = webApplicationFactory;
        }

        public WebApplicationInfrastructure()
        {
            _webApplicationFactory = new WebApplicationFactory<TEntryPoint>();
        }

        public override int Order => 999;

        public override void Destroy()
        {
            _webApplicationFactory.Dispose();
        }

        public override void Initialize()
        {
            HttpClient = _webApplicationFactory.CreateDefaultClient();
        }

        public override void Reset()
        {
        }
    }
}
