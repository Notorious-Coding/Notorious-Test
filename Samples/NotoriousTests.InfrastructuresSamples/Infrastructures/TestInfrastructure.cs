using NotoriousTest.Infrastructures.Async;

using System.Text.Json;

namespace NotoriousTests.InfrastructuresSamples.Infrastructures;

public class Toto
{
    public string Titi { get; set; }
}
public class Config
{
    public Toto Toto { get; set; }
}



public class TestInfrastructure : ExternalAsyncInfrastructure<Dictionary<string, string>, Config>
{

    public override Task CleanUp(Config restoredConfiguration)
    {
        Console.WriteLine("Clean up TestInfra | Config :" + JsonSerializer.Serialize(restoredConfiguration));
        return Task.CompletedTask;
    }

    public override Task Destroy()
    {
        return Task.CompletedTask;
    }

    public override Task Initialize()
    {
        return Task.CompletedTask;
    }

    public override Task Reset()
    {
        return Task.CompletedTask;
    }
}

