using NotoriousTest.Common.Infrastructures;
using NotoriousTest.Common.Exceptions;
using Xunit;
using Microsoft.AspNetCore.Mvc;
using NotoriousTest.Common.Infrastructures.Async;

namespace NotoriousTest.Common.Environments
{
    public abstract class AsyncEnvironment : IAsyncLifetime
    {
        public Guid EnvironmentId => Guid.NewGuid();

        protected readonly List<AsyncInfrastructure> Infrastructures = new List<AsyncInfrastructure>();
        protected Dictionary<string, string> Configuration = new Dictionary<string, string>();

        #region IAsyncLifetime Implementation


        /// <summary>
        /// Initialize environment by creating infrastructure. THIS METHOD IS CALLED BY XUNIT, DO NOT USE IT.
        /// </summary>
        public virtual async Task InitializeAsync()
        {
            await ConfigureEnvironmentAsync();
            await Initialize();
        }

        /// <summary>
        /// Destroy every created infrastructure. THIS METHOD IS CALLED BY XUNIT, DO NOT USE IT.
        /// </summary>
        public async Task DisposeAsync()
        {
            await Destroy();
        }
        #endregion

        /// <summary>
        /// Configure environment with infrastructures. Called before environment initialization.
        /// </summary>
        public abstract Task ConfigureEnvironmentAsync();

       
        /// <summary>
        /// Get an infrastructure within environment.
        /// </summary>
        /// <typeparam name="T">Infrastructure type</typeparam>
        /// <returns>Infrastructure of type <typeparamref name="T"/></returns>
        /// <exception cref="InfrastructureNotFoundException">Infrastructure has not beed found within environment.</exception>
        public Task<T> GetInfrastructureAsync<T>() where T : AsyncInfrastructure
        {
            T? infrastructure = Infrastructures.OfType<T>().FirstOrDefault();

            if (infrastructure == null) throw new InfrastructureNotFoundException($"L'infrastructure persistante de type {typeof(T)} n'éxiste pas, veuillez vérififer la méthode ${nameof(ConfigureEnvironmentAsync)}");

            return Task.FromResult(infrastructure);
        }

        /// <summary>
        /// Add an infrastructure within environment.
        /// </summary>
        /// <param name="infrastructure"></param>
        public Task<AsyncEnvironment> AddInfrastructure(AsyncInfrastructure infrastructure)
        {
            Infrastructures.Add(infrastructure); 
            return Task.FromResult(this);
        } 

        internal async Task Initialize()
        {
            foreach (AsyncInfrastructure infra in Infrastructures.OrderBy((i) => i.Order))
            {
                await infra.Initialize();
                if (infra is IConfigurable){

                    Configuration = Configuration.Concat((infra as IConfigurable).GetConfigurationAsDictionary()).ToDictionary(k => k.Key, v => v.Value);
                }
            }
        }

        internal async Task Reset()
        {
            foreach (AsyncInfrastructure infrastructure in Infrastructures.OrderBy(pi => pi.Order))
            {
                await infrastructure.Reset();
            }
        }

        internal async Task Destroy()
        {
            foreach (AsyncInfrastructure infra in Infrastructures.OrderBy(i => i.Order))
            {
                await infra.Destroy();
            }
        }
    }
}
