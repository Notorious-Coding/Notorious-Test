using Microsoft.AspNetCore.Mvc.Testing;
using NotoriousTest.Common.Infrastructures.Sync;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotoriousTest.Web.Infrastructures
{
    public class WebApplicationInfrastructure<TEntryPoint> : Infrastructure where TEntryPoint : class
    {
        private WebApplicationFactory<TEntryPoint> _webApplicationFactory;
        public HttpClient? HttpClient;
        public override int Order => 999;

        public WebApplicationInfrastructure(WebApplicationFactory<TEntryPoint> webApplicationFactory)
        {
            _webApplicationFactory = webApplicationFactory;
        }

        public WebApplicationInfrastructure()
        {
            _webApplicationFactory = new WebApplicationFactory<TEntryPoint>();
        }


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
