using NotoriousTest.Common.Exceptions;
using NotoriousTest.Common.Infrastructures.Sync;

namespace NotoriousTest.Common.Environments
{
    public abstract class Environment : IDisposable
    {
        public Guid EnvironmentId { get; private set; } = Guid.NewGuid();

        protected readonly List<Infrastructure> Infrastructures = new List<Infrastructure>();

        public Environment()
        {
            // Called before test campaign
            InitializeEnvironment();
        }

        public virtual void InitializeEnvironment()
        {
            ConfigureEnvironment();
            Initialize();
        }

        /// <summary>
        /// Configure environment with infrastructures. Called before environment initialization.
        /// </summary>
        public abstract void ConfigureEnvironment();

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

        /// <summary>
        /// Get an infrastructure within environment.
        /// </summary>
        /// <typeparam name="T">Infrastructure type</typeparam>
        /// <returns>Infrastructure of type <typeparamref name="T"/></returns>
        /// <exception cref="InfrastructureNotFoundException">Infrastructure has not beed found within environment.</exception>
        public T GetInfrastructure<T>() where T : Infrastructure
        {
            T? infrastructure = Infrastructures.OfType<T>().FirstOrDefault();

            if (infrastructure == null) throw new InfrastructureNotFoundException($"L'infrastructure persistante de type {nameof(T)} n'éxiste pas, veuillez vérififer la méthode ${nameof(ConfigureEnvironment)}");

            return infrastructure;
        }

        public void Initialize()
        {
            foreach (Infrastructure infra in Infrastructures.OrderBy(pi => pi.Order))
            {
                infra.Initialize();
            }
        }

        public void Reset()
        {
            foreach (Infrastructure infrastructure in Infrastructures.OrderBy(pi => pi.Order))
            {
                if(infrastructure.AutoReset) infrastructure.Reset();
            }
        }

        public void Destroy()
        {
            foreach (Infrastructure infra in Infrastructures.OrderBy(pi => pi.Order))
            {
                infra.Destroy();
            }
        }

        /// <summary>
        /// Destroy every created infrastructure. THIS METHOD IS CALLED BY XUNIT, DO NOT USE IT.
        /// </summary>
        public void Dispose()
        {
            Destroy();
        }
    }
}
