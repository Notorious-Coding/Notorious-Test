using AwesomeAssertions;
using FakeItEasy;
using NotoriousTest.Core;
using NotoriousTest.Core.Logger;
using NotoriousTest.Core.Registry;

namespace NotoriousTest.IntegrationTests.Npm;

public class NpmInfrastructureTests
{
    [Fact]
    public async Task Initialize_Should_Start_Server()
    {
        EnvironmentId contextId = Guid.NewGuid();
        await using var infrastructure =
            new TestNpmApplicationInfrastructure(contextId, A.Fake<ITestLogger>(), A.Fake<IRegistry>());

        await infrastructure.InitializeAsync();

        infrastructure.HttpClient.Should().NotBeNull();
        HttpResponseMessage response =
            await infrastructure.HttpClient.GetAsync("/health", TestContext.Current.CancellationToken);

        Func<HttpResponseMessage> act = () => response.EnsureSuccessStatusCode();

        act.Should().NotThrow();
    }

    [Fact]
    public async Task Destroy_Should_Kill_Server()
    {
        EnvironmentId contextId = Guid.NewGuid();
        var infrastructure =
            new TestNpmApplicationInfrastructure(contextId, A.Fake<ITestLogger>(), A.Fake<IRegistry>());

        await infrastructure.InitializeAsync();

        infrastructure.HttpClient.Should().NotBeNull();
        HttpResponseMessage response =
            await infrastructure.HttpClient.GetAsync("/health", TestContext.Current.CancellationToken);

        response.IsSuccessStatusCode.Should().BeTrue();

        Uri? baseAddress = infrastructure.HttpClient.BaseAddress;
        await infrastructure.DestroyAsync();
        using var independentClient = new HttpClient { BaseAddress = baseAddress };

        Func<Task<HttpResponseMessage>> act = () =>
            independentClient.GetAsync("/health", TestContext.Current.CancellationToken);
        await act.Should().ThrowAsync<HttpRequestException>();
    }
}
