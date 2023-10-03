using NotoriousTest.Common.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotoriousTest.Common.Infrastructures.Sync
{
    public abstract class ConfigurableInfrastructure<TConfiguration> : Infrastructure, IConfigurable<TConfiguration> where TConfiguration : class, new()
    {
        public TConfiguration Configuration { get; set; } = new();
        public ConfigurableInfrastructure(bool initialize = false) : base(initialize)
        {

        }

        public virtual Dictionary<string, string> GetConfigurationAsDictionary()
        {
            return Configuration.ToDictionary();
        }

    }

    public abstract class ConfigurableInfrastructure : ConfigurableInfrastructure<Dictionary<string, string>>
    {
        public ConfigurableInfrastructure(bool initialize = false) : base(initialize)
        {

        }

        public override Dictionary<string, string> GetConfigurationAsDictionary()
        {
            return Configuration;
        }
    }
}
