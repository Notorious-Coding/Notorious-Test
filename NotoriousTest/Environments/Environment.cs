using NotoriousTest.Exceptions;
using NotoriousTest.Infrastructures;

using Xunit;

namespace NotoriousTest.Environments
{
    public abstract class Environment : IAsyncLifetime
    {
        public Guid EnvironmentId { get; private set; } = Guid.NewGuid();

        protected readonly List<Infrastructure> Infrastructures = new List<Infrastructure>();

        #region IAsyncLifetime Implementation

        /// <summary>
        /// Initialize environment. THIS METHOD IS CALLED BY XUNIT, DO NOT USE IT.
        /// </summary>
        public async ValueTask InitializeAsync()
        {
            await ConfigureEnvironment();
            await Initialize();
        }

        /// <summary>
        /// Destroy environment. THIS METHOD IS CALLED BY XUNIT, DO NOT USE IT.
        /// </summary>
        public async ValueTask DisposeAsync()
        {
            await Destroy();
        }
        #endregion

        /// <summary>
        /// Configure environment with infrastructures. Called before environment initialization.
        /// </summary>
        public abstract Task ConfigureEnvironment();


        /// <summary>
        /// Get an infrastructure within environment.
        /// </summary>
        /// <typeparam name="T">Infrastructure type</typeparam>
        /// <returns>Infrastructure of type <typeparamref name="T"/></returns>
        /// <exception cref="InfrastructureNotFoundException">Infrastructure has not beed found within environment.</exception>
        public T GetInfrastructure<T>() where T : Infrastructure
        {
            T? infrastructure = Infrastructures.OfType<T>().FirstOrDefault();

            if (infrastructure == null) throw new InfrastructureNotFoundException($"L'infrastructure persistante de type {typeof(T)} n'éxiste pas, veuillez vérififer la méthode ${nameof(ConfigureEnvironment)}");

            return infrastructure;
        }

        /// <summary>
        /// Add an infrastructure within environment.
        /// </summary>
        /// <param name="infrastructure"></param>
        public Environment AddInfrastructure(Infrastructure infrastructure)
        {
            infrastructure.ContextId = EnvironmentId;
            Infrastructures.Add(infrastructure);
            return this;
        }

        public virtual async Task Initialize()
        {
            foreach (Infrastructure infra in Infrastructures.OrderBy((i) => i.Order))
            {
                await infra.Initialize();
            }
        }

        public virtual async Task Reset()
        {
            foreach (Infrastructure infrastructure in Infrastructures.OrderBy(pi => pi.Order))
            {
                if (infrastructure.AutoReset) await infrastructure.Reset();
            }
        }

        public virtual async Task Destroy()
        {
            foreach (Infrastructure infra in Infrastructures.OrderBy(i => i.Order))
            {
                await infra.Destroy();
            }
        }
    }
}
