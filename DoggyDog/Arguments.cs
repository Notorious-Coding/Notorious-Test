
namespace NotoriousTest.DoggyDog
{
    internal record Arguments(int Pid, string AssemblyPath, string ConnectionString, Guid EnvironmentId)
    {
        public static Arguments From(string[] args)
        {
#if DEBUG
            Logger.Magenta(() => Console.WriteLine("/!\\ DEBUG MODE /!\\"));
            string debugcs = Environment.GetEnvironmentVariable("DEBUG_DOGGYDOG_CS", EnvironmentVariableTarget.User);
            string debugenv = Environment.GetEnvironmentVariable("DEBUG_DOGGYDOG_ENVIRONMENT", EnvironmentVariableTarget.User);
            string debugassembly = Environment.GetEnvironmentVariable("DEBUG_DOGGYDOG_ASSEMBLY", EnvironmentVariableTarget.User);
            string debugpid = Environment.GetEnvironmentVariable("DEBUG_DOGGYDOG_PID", EnvironmentVariableTarget.User);
            Logger.Magenta(() => Console.WriteLine("[DEBUG] PARAMETRE DE DEBUG : "));
            Logger.Magenta(() => Console.WriteLine($"[DEBUG] ConnectionString : {debugcs} "));
            Logger.Magenta(() => Console.WriteLine($"[DEBUG] Environment : {debugenv} "));
            Logger.Magenta(() => Console.WriteLine($"[DEBUG] Assembly : {debugassembly} "));
            Logger.Magenta(() => Console.WriteLine($"[DEBUG] PID : {debugpid} "));

            return new Arguments(int.Parse(debugpid), debugassembly, debugcs, Guid.Parse(debugenv));
#endif

            static Dictionary<string, string> ParseArgs(string[] args)
            {
                var result = new Dictionary<string, string>();
                for (int i = 0; i < args.Length - 1; i++)
                {
                    if (args[i].StartsWith("--"))
                        result[args[i][2..]] = args[i + 1];
                }
                return result;
            }

            var parsedArgs = ParseArgs(args);

            if (!parsedArgs.TryGetValue("pid", out string? pidStr))
            {
                Logger.Red(() => Console.WriteLine("[DoggyDog]--pid parameter is missing."));
            }

            if (!int.TryParse(pidStr, out int pid))
            {
                Logger.Red(() => Console.WriteLine("[DoggyDog] --pid is not a number."));
            }

            if (!parsedArgs.TryGetValue("assembly", out string? assemblyPath))
            {
                Logger.Red(() => Console.WriteLine("[DoggyDog] --assembly parameter is missing."));
            }

            if (!parsedArgs.TryGetValue("connectionString", out string? connectionString))
            {
                Logger.Red(() => Console.WriteLine("[DoggyDog] --connectionString parameter is missing."));
            }

            if (!parsedArgs.TryGetValue("environment", out string? environmentId))
            {
                Logger.Red(() => Console.WriteLine("[DoggyDog] --environment parameter is missing."));
            }

            if (pid == default || assemblyPath == null || connectionString == null || environmentId == null)
            {
                Environment.Exit(-1);
            }



            return new Arguments(pid, assemblyPath, connectionString, Guid.Parse(environmentId));
        }
    }
}

