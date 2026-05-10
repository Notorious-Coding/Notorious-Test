using DoggyDog.Logs;

namespace DoggyDog
{
    internal record WatchDogParameters
    {
        [CliArgument("pid")]
        public int Pid { get; set; }

        [CliArgument("assembly")]
        public string AssemblyPath { get; set; }

        [CliArgument("connectionString")]
        public string ConnectionString { get; set; }

        [CliArgument("environment")]
        public Guid EnvironmentId { get; set; }

        [CliArgument("runtimes", Required: false)]
        public string[] RuntimesPath { get; set; }

        [CliArgument("loglevel", Required: false)]
        public LogLevel? LogLevel { get; set; }

        internal void Deconstruct(out int pid, out string assemblyPath, out string connectionString, out Guid environmentId, out string[] runtimes)
        {
            pid = Pid;
            assemblyPath = AssemblyPath;
            connectionString = ConnectionString;
            environmentId = EnvironmentId;
            runtimes = RuntimesPath;
        }
    }
}

