// DoggyDog/Banner.cs
namespace NotoriousTest.DoggyDog;

internal static class Banner
{

    private const string Title = """
          ██████╗  ██████╗  ██████╗  ██████╗ ██╗   ██╗    ██████╗  ██████╗  ██████╗ 
          ██╔══██╗██╔═══██╗██╔════╝ ██╔════╝╚ ██╗ ██╔╝    ██╔══██╗██╔═══██╗██╔════╝ 
          ██║  ██║██║   ██║██║  ███╗██║  ███╗ ╚████╔╝     ██║  ██║██║   ██║██║  ███╗
          ██║  ██║██║   ██║██║   ██║██║   ██║  ╚██╔╝      ██║  ██║██║   ██║██║   ██║
          ██████╔╝╚██████╔╝╚██████╔╝╚██████╔╝   ██║       ██████╔╝╚██████╔╝╚██████╔╝
          ╚═════╝  ╚═════╝  ╚═════╝  ╚═════╝    ╚═╝       ╚═════╝  ╚═════╝  ╚═════╝ 
        """;

    public static void Print(string version, int parentPid, Guid environmentId)
    {
        Logger.Magenta(() =>
        {
            Console.WriteLine(Title);
        });

        Logger.DarkGray(() =>
        {
            Console.WriteLine($"  Watchdog process  ·  Monitoring PID {parentPid} | EID {environmentId}  ·  v{version}");
            Console.WriteLine();
        });
    }
}