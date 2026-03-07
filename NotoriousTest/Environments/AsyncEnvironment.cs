using NotoriousTest.Exceptions;
using NotoriousTest.Infrastructures.Async;

using System.Diagnostics;
using System.Reflection;

using Xunit;

namespace NotoriousTest.Environments
{
    public abstract class AsyncEnvironment : IAsyncLifetime
    {
        public Guid EnvironmentId { get; private set; } = Guid.NewGuid();
        public abstract Assembly CurrentAssembly { get; }

        protected readonly List<AsyncInfrastructure> Infrastructures = new List<AsyncInfrastructure>();

        #region IAsyncLifetime Implementation

        /// <summary>
        /// Initialize environment. THIS METHOD IS CALLED BY XUNIT, DO NOT USE IT.
        /// </summary>
        public async Task InitializeAsync()
        {
            StartDoggyDogWatcher();

            await ConfigureEnvironmentAsync();
            await Initialize();
        }

        private void StartDoggyDogWatcher()
        {
            var watchdogPath = Path.Combine(AppContext.BaseDirectory, "NotoriousTest.DoggyDog.exe");
            var configPath = Path.Combine(AppContext.BaseDirectory, "tests.appsettings.json");
            var currentPid = Process.GetCurrentProcess().Id;
            var assemblyPath = CurrentAssembly.Location;

            Process.Start(new ProcessStartInfo
            {
                FileName = watchdogPath,
                Arguments = $"--pid {currentPid} --assembly \"{assemblyPath}\" --config \"{configPath}\"",
                UseShellExecute = true,
                CreateNoWindow = false,
            });
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
        /// Configure environment with infrastructures. Called before environment initialization.
        /// </summary>
        public abstract Task ConfigureEnvironmentAsync();


        /// <summary>
        /// Get an infrastructure within environment.
        /// </summary>
        /// <typeparam name="T">Infrastructure type</typeparam>
        /// <returns>Infrastructure of type <typeparamref name="T"/></returns>
        /// <exception cref="InfrastructureNotFoundException">Infrastructure has not beed found within environment.</exception>
        public T GetInfrastructure<T>() where T : AsyncInfrastructure
        {
            T? infrastructure = Infrastructures.OfType<T>().FirstOrDefault();

            if (infrastructure == null) throw new InfrastructureNotFoundException($"L'infrastructure persistante de type {typeof(T)} n'éxiste pas, veuillez vérififer la méthode ${nameof(ConfigureEnvironmentAsync)}");

            return infrastructure;
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
