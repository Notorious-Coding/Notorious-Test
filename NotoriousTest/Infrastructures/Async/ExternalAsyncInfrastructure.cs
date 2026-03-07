using NotoriousTest.Configuration;

namespace NotoriousTest.Infrastructures.Async
{
    public abstract class ExternalAsyncInfrastructure<TOutputConfiguration, TInputConfiguration> : AsyncInfrastructure, IConfigurableInfrastructure<TOutputConfiguration, TInputConfiguration> where TOutputConfiguration : class where TInputConfiguration : class
    {
        public TOutputConfiguration OutputConfiguration { get; set; }
        public TInputConfiguration InputConfiguration { get; set; }

        /// <summary>
        /// This method is called when tests are canceled brutally. This is used to clean the infrastructure using the configuration parameters.
        /// This method cannot used properties of this class, they will not be accessible.
        /// If destroy is stateless, then CleanUp can call Destroy.
        /// </summary>
        /// <param name="configuration">Tests configuration.</param>
        public virtual Task CleanUp(TInputConfiguration configuration)
        {
            return Task.CompletedTask;
        }
    }
}
