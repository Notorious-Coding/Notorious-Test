namespace DoggyDog.Watchdog.Logs;

public static class LogColorScope
{
    public static void WithColor(ConsoleColor color, Action logs)
    {
        Console.ForegroundColor = color;
        logs();
        Console.ResetColor();
    }

    public static void Black(Action logs) => WithColor(ConsoleColor.Black, logs);
    public static void DarkBlue(Action logs) => WithColor(ConsoleColor.DarkBlue, logs);
    public static void DarkGreen(Action logs) => WithColor(ConsoleColor.DarkGreen, logs);
    public static void DarkCyan(Action logs) => WithColor(ConsoleColor.DarkCyan, logs);
    public static void DarkRed(Action logs) => WithColor(ConsoleColor.DarkRed, logs);
    public static void DarkMagenta(Action logs) => WithColor(ConsoleColor.DarkMagenta, logs);
    public static void DarkYellow(Action logs) => WithColor(ConsoleColor.DarkYellow, logs);
    public static void Gray(Action logs) => WithColor(ConsoleColor.Gray, logs);
    public static void DarkGray(Action logs) => WithColor(ConsoleColor.DarkGray, logs);
    public static void Blue(Action logs) => WithColor(ConsoleColor.Blue, logs);
    public static void Green(Action logs) => WithColor(ConsoleColor.Green, logs);
    public static void Cyan(Action logs) => WithColor(ConsoleColor.Cyan, logs);
    public static void Red(Action logs) => WithColor(ConsoleColor.Red, logs);
    public static void Magenta(Action logs) => WithColor(ConsoleColor.Magenta, logs);
    public static void Yellow(Action logs) => WithColor(ConsoleColor.Yellow, logs);
    public static void White(Action logs) => WithColor(ConsoleColor.White, logs);
}
