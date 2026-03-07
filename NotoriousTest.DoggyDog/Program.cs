// See https://aka.ms/new-console-template for more information

using NotoriousTest.DoggyDog;
using NotoriousTest.Infrastructures.Async;

using System.Diagnostics;
using System.Reflection;
using System.Text.Json;

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
try
{
    Console.WriteLine("Args reçus: " + string.Join(" | ", args));

    Arguments arguments = Arguments.From(args);

    var parent = Process.GetProcessById(arguments.pid);

    Console.WriteLine("Waiting for parent process to Exit; Pid = {0}", arguments.pid);
    parent.WaitForExit();
    Console.WriteLine("Parent process terminated; Loading assembly...");


    var assemblyDir = Path.GetDirectoryName(arguments.assemblyPath)!;

    AppDomain.CurrentDomain.AssemblyResolve += (sender, eventArgs) =>
    {
        var assemblyName = new AssemblyName(eventArgs.Name).Name + ".dll";
        var path = Path.Combine(assemblyDir, assemblyName);
        return File.Exists(path) ? Assembly.LoadFrom(path) : null;
    };

    Assembly assembly = Assembly.LoadFrom(arguments.assemblyPath);

    Console.WriteLine("Assembly loaded...");

    IEnumerable<Type> infraTypes = InfrastructureLoader.LoadExternalInfrastructure(assembly);

    IEnumerable<object?> infras = infraTypes.Select(Activator.CreateInstance).Where(t => t != null);
    Console.WriteLine(JsonSerializer.Serialize(infras));
    var configStr = File.ReadAllText(arguments.configPath);

    static Type GetExternalInfrastructureBase(Type type)
    {
        while (type != null)
        {
            Console.WriteLine("GetExternalInfrastructureBase" + type.ToString());
            if (type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(ExternalInfrastructure<,>) || type.GetGenericTypeDefinition() == typeof(ExternalAsyncInfrastructure<,>)))
            {
                return type;
            }
            type = type.BaseType;
        }
        return null;

    }


    foreach (object infra in infras)
    {
        Console.WriteLine("Calling clean up on {0}", infra.GetType());
        Type type = GetExternalInfrastructureBase(infra.GetType());
        Console.WriteLine("Salut" + type.ToString());
        Type inputConfigType = type.GetGenericArguments()[1];

        object configuration = JsonSerializer.Deserialize(configStr, inputConfigType);
        Console.WriteLine("Configuration" + configStr);
        var cleanUpMethod = type.GetMethod("CleanUp");

        var result = cleanUpMethod.Invoke(infra, new[] { configuration });

        if (result is Task task)
        {
            await task;
        }
        Console.WriteLine("Clean up done", infra.GetType());
    }
    Console.ReadKey();


}
catch (ReflectionTypeLoadException ex)
{
    foreach (var loaderException in ex.LoaderExceptions)
    {
        Console.WriteLine(loaderException?.ToString());
        Console.ReadKey();
    }
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine(ex);
    Console.ReadKey();
}