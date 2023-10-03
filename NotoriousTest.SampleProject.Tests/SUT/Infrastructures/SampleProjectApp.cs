using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using NotoriousTest.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotoriousTest.SampleProject.Tests.SUT.Infrastructures
{
    internal class SampleProjectApp : ConfiguredWebApplication<Program>
    {

    }
}
