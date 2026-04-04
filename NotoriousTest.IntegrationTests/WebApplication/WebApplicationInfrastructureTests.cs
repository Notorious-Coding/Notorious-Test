using AwesomeAssertions;

using FakeItEasy;


using NotoriousTest.Core;
using NotoriousTest.Core.Logger;
using NotoriousTest.Core.Registry;
using NotoriousTest.Web.Infrastructures;

namespace NotoriousTest.IntegrationTests.WebApplication
{
    public class WebApplicationInfrastructureTests
    {
        [Fact]
        public async Task Initialize_Should_Start_Server()
        {
            ContextId contextId = Guid.NewGuid();
            var infrastructure = new WebApplicationInfrastructure<TestWebApplication>(contextId, A.Fake<ITestLogger>(), A.Fake<IRegistry>());

            await infrastructure.InitializeAsync();

            infrastructure.HttpClient.Should().NotBeNull();
            HttpResponseMessage response = await infrastructure.HttpClient.GetAsync("/health", TestContext.Current.CancellationToken);

            var act = () => response.EnsureSuccessStatusCode();

            act.Should().NotThrow();
        }

        [Fact]
        public async Task Destroy_Should_Kill_Server()
        {
            ContextId contextId = Guid.NewGuid();
            var infrastructure = new WebApplicationInfrastructure<TestWebApplication>(contextId, A.Fake<ITestLogger>(), A.Fake<IRegistry>());

            await infrastructure.InitializeAsync();

            infrastructure.HttpClient.Should().NotBeNull();
            HttpResponseMessage response = await infrastructure.HttpClient.GetAsync("/health", TestContext.Current.CancellationToken);

            response.IsSuccessStatusCode.Should().BeTrue();

            var baseAddress = infrastructure.HttpClient.BaseAddress;
            await infrastructure.DestroyAsync();
            using var independentClient = new HttpClient { BaseAddress = baseAddress };

            var act = () => independentClient.GetAsync("/health", TestContext.Current.CancellationToken);
            await act.Should().ThrowAsync<HttpRequestException>();
        }
    }
}
