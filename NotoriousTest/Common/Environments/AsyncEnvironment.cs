using NotoriousTest.Common.Exceptions;
using NotoriousTest.Common.Infrastructures.Async;
using Xunit;

namespace NotoriousTest.Common.Environments
{
    public abstract class AsyncEnvironment : IAsyncLifetime
    {
        public Guid EnvironmentId { get; private set; } = Guid.NewGuid();

        protected readonly List<AsyncInfrastructure> Infrastructures = new List<AsyncInfrastructure>();

        #region IAsyncLifetime Implementation

        /// <summary>
        /// Initialize environment. THIS METHOD IS CALLED BY XUNIT, DO NOT USE IT.
        /// </summary>
        public async Task InitializeAsync()
        {
            await Initialize();
        }

        /// <summary>
        /// Destroy environment. THIS METHOD IS CALLED BY XUNIT, DO NOT USE IT.
        /// </summary>
        public async Task DisposeAsync()
        {
            await Destroy();
        }
        #endregion

        /// <summary>
        /// Initialize environment by creating infrastructure.
        /// </summary>
        /// <returns></returns>
        public virtual async Task InitializeEnvironmentAsync()
        {
            await ConfigureEnvironmentAsync();
            await Initialize();
        }

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
            infrastructure.ContextId = EnvironmentId;
            Infrastructures.Add(infrastructure); 
            return Task.FromResult(this);
        } 

        public virtual async Task Initialize()
        {

            foreach (AsyncInfrastructure infra in Infrastructures.OrderBy((i) => i.Order))
            {
                await infra.Initialize();
            }
        }

        public virtual async Task Reset()
        {
            foreach (AsyncInfrastructure infrastructure in Infrastructures.OrderBy(pi => pi.Order))
            {
                if (infrastructure.AutoReset) await infrastructure.Reset();
            }
        }

        public virtual async Task Destroy()
        {
            foreach (AsyncInfrastructure infra in Infrastructures.OrderBy(i => i.Order))
            {
                await infra.Destroy();
            }
        }
    }
}
