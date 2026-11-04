using System.Diagnostics;
using AwesomeAssertions;
using NotoriousTest.Processes;

namespace NotoriousTest.UnitTests.Processes;

public class ProcessInfrastructureCleanerTests
{
    private static Process StartLongRunningProcess()
    {
        ProcessStartInfo psi = OperatingSystem.IsWindows()
            ? new ProcessStartInfo("ping", "-n 60 127.0.0.1")
            : new ProcessStartInfo("sleep", "60");
        psi.UseShellExecute = false;
        psi.CreateNoWindow = true;
        psi.RedirectStandardOutput = true;
        return Process.Start(psi)!;
    }

    [Fact]
    public async Task CleanAfterCrash_Should_Kill_Process()
    {
        using Process process = StartLongRunningProcess();

        await new ProcessInfrastructureCleaner().CleanAfterCrash(Guid.NewGuid(), Guid.NewGuid(),
            ProcessMetadata.From(process));

        process.WaitForExit(TimeSpan.FromSeconds(10)).Should().BeTrue();
    }

    [Fact]
    public async Task CleanAfterCrash_Should_Not_Kill_Process_When_Pid_Has_Been_Reused()
    {
        using Process process = StartLongRunningProcess();
        try
        {
            ProcessMetadata metadata = ProcessMetadata.From(process);
            metadata.StartTimeUtc = metadata.StartTimeUtc.AddMinutes(-5);

            await new ProcessInfrastructureCleaner().CleanAfterCrash(Guid.NewGuid(), Guid.NewGuid(), metadata);

            process.HasExited.Should().BeFalse();
        }
        finally
        {
            process.Kill(true);
        }
    }

    [Fact]
    public async Task CleanAfterCrash_Should_Do_Nothing_When_Process_Has_Already_Exited()
    {
        using Process process = StartLongRunningProcess();
        ProcessMetadata metadata = ProcessMetadata.From(process);
        process.Kill(true);
        await process.WaitForExitAsync();

        Func<Task> clean = () => new ProcessInfrastructureCleaner().CleanAfterCrash(Guid.NewGuid(), Guid.NewGuid(),
            metadata);

        await clean.Should().NotThrowAsync();
    }
}
