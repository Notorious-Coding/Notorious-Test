using NotoriousTest.Configuration;
using NotoriousTest.Infrastructures.Sync;

namespace NotoriousTest.Infrastructures.Async
{
    public abstract class ExternalInfrastructure<TOutputConfiguration, TInputConfiguration> : Infrastructure, IConfigurableInfrastructure<TOutputConfiguration, TOutputConfiguration> where TOutputConfiguration : class where TInputConfiguration : class
    {
        public TOutputConfiguration OutputConfiguration { get; set; }
        public TOutputConfiguration InputConfiguration { get; set; }

        /// <summary>
        /// Clean the ressource to the external provider when tests crash unexpectedly. MUST BE STATELESS.
        /// </summary>
        /// <param name="restoredConfiguration">Restored configuration.</param>
        public virtual void CleanUp(TOutputConfiguration restoredConfiguration)
        {
        }
    }
}
