
namespace NotoriousTest.DoggyDog
{
    internal record Arguments(int Pid, string AssemblyPath)
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

            Console.ForegroundColor = ConsoleColor.Red;
            if (!parsedArgs.TryGetValue("pid", out string? pidStr))
            {
                Console.WriteLine("Error: --pid parameters is missing.");
            }

            if (!int.TryParse(pidStr, out int pid))
            {
                Console.WriteLine("Error: --pid is not a number.");
            }

            if (!parsedArgs.TryGetValue("assembly", out string? assemblyPath))
            {
                Console.WriteLine("Error: --assembly parameters is missing.");
            }

            if (pid == default || assemblyPath == null)
            {
                Environment.Exit(-1);
            }

            Console.ForegroundColor = ConsoleColor.White;

            return new Arguments(pid, assemblyPath);
        }
    }
}

