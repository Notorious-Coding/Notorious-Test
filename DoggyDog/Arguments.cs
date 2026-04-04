
namespace NotoriousTest.DoggyDog
{
    internal record Arguments(int Pid, string AssemblyPath, string ConnectionString)
    {
        public static Arguments From(string[] args)
        {
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

            if (pid == default || assemblyPath == null || connectionString == null)
            {
                Environment.Exit(-1);
            }



            return new Arguments(pid, assemblyPath, connectionString);
        }
    }
}

