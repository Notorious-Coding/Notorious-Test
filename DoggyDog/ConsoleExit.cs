using System.Diagnostics;
using System.Runtime.InteropServices;
using DoggyDog.Logs;

namespace DoggyDog;

/// <summary>
///     Handles the end of a DoggyDog session : the window closes automatically after a delay,
///     unless the user focuses it, in which case it stays open until a key is pressed.
/// </summary>
internal static class ConsoleExit
{
    private const uint GA_ROOTOWNER = 3;
    private static readonly TimeSpan AutoCloseDelay = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan PollInterval = TimeSpan.FromMilliseconds(100);

    [DllImport("kernel32.dll")]
    private static extern IntPtr GetConsoleWindow();

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern IntPtr GetAncestor(IntPtr hwnd, uint flags);

    public static void ExitAfterDelay(int exitCode, Logger logger)
    {
        logger.Info($"Closing in {AutoCloseDelay.TotalSeconds:0} seconds. Focus this window to keep it open.");

        var stopwatch = Stopwatch.StartNew();
        while (stopwatch.Elapsed < AutoCloseDelay)
        {
            if (IsConsoleFocused())
            {
                logger.Info("Console focused. Stopping auto-close.");
                break;
            }

            Thread.Sleep(PollInterval);
        }

        stopwatch.Stop();
        if (stopwatch.Elapsed < AutoCloseDelay)
        {
            Console.WriteLine("Press any key to exit...");
            WaitForKey();
        }

        Environment.Exit(exitCode);
    }

    private static bool IsConsoleFocused()
    {
        if (!OperatingSystem.IsWindows())
            return false;

        IntPtr consoleWindow = GetConsoleWindow();
        if (consoleWindow == IntPtr.Zero)
            return false;

        IntPtr foreground = GetForegroundWindow();

        // With Windows Terminal, the console window is a hidden pseudo-window owned by the terminal window.
        return foreground == consoleWindow || foreground == GetAncestor(consoleWindow, GA_ROOTOWNER);
    }

    private static void WaitForKey()
    {
        try
        {
            Console.ReadLine();
        }
        catch (InvalidOperationException)
        {
            // Input is redirected: no key to wait for.
        }
    }
}
