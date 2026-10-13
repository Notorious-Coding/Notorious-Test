using DoggyDog.Logs;

namespace NotoriousTest.Internal.Watchdog;

public class DoggyDogWatchdogConfiguration
{
    public const string SECTION_NAME = "Watchdog";
    public bool HideDoggyDog { get; set; } = false;
    public bool ManualLaunch { get; set; } = false;
    public LogLevel MinLogLevel { get; set; } = LogLevel.Info;
}
