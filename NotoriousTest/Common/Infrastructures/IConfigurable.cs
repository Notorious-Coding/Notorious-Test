using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotoriousTest.Common.Infrastructures
{
    public interface IConfigurable<TConfiguration> : IConfigurable
    {
        TConfiguration Configuration { get; set; }
    }

    public interface IConfigurable
    {
        public Dictionary<string, string> GetConfigurationAsDictionary();
    }
}