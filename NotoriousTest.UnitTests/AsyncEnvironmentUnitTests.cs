using FakeItEasy;
using NotoriousTest.Common.Environments;
using NotoriousTest.Common.Infrastructures.Async;

namespace NotoriousTest.UnitTests
{

    public class AsyncEnvironmentUnitTests
    {

        static TestInfrastructure mockedInfra;

        public class TestEnvironment : AsyncEnvironment
        {
            public TestEnvironment()
            {
                
            }

            public override Task ConfigureEnvironmentAsync()
            {
                AddInfrastructure(mockedInfra);

                return Task.CompletedTask;
            }
        }   

        public abstract class TestInfrastructure : AsyncInfrastructure
        {

            public TestInfrastructure() : base(false)
            {
                
            }
        }


        [Fact]
        public async Task GetInfrastructure_Should_ReturnProperInfrastructure()
        {
            mockedInfra = A.Fake<TestInfrastructure>();
            var environment = new TestEnvironment();
            await environment.InitializeAsync();
            TestInfrastructure infra = await environment.GetInfrastructureAsync<TestInfrastructure>();

            Assert.NotNull(infra);
            Assert.Equal(environment.EnvironmentId, infra.ContextId);
        }

        [Fact]
        public async Task EnvironmentCreation_Should_CallInfrastructureInitialization()
        {
            mockedInfra = A.Fake<TestInfrastructure>();

            var environment = new TestEnvironment();

            await environment.InitializeAsync();

            A.CallTo(() => mockedInfra.Initialize()).MustHaveHappenedOnceExactly();
            A.CallTo(() => mockedInfra.Reset()).MustNotHaveHappened();
            A.CallTo(() => mockedInfra.Destroy()).MustNotHaveHappened();
        }

        [Fact]
        public async Task Reset_Should_CallInfrastructureReset()
        {
            mockedInfra = A.Fake<TestInfrastructure>();

            var environment = new TestEnvironment();

            await environment.InitializeAsync();
            await environment.Reset();

            A.CallTo(() => mockedInfra.Initialize()).MustHaveHappenedOnceExactly();
            A.CallTo(() => mockedInfra.Reset()).MustHaveHappenedOnceExactly();
            A.CallTo(() => mockedInfra.Destroy()).MustNotHaveHappened();
        }

        [Fact]
        public async Task Destroy_Should_CallInfrastructureDestroy()
        {
            mockedInfra = A.Fake<TestInfrastructure>();

            var environment = new TestEnvironment();

            await environment.InitializeAsync();
            await environment.Destroy();

            A.CallTo(() => mockedInfra.Initialize()).MustHaveHappenedOnceExactly();
            A.CallTo(() => mockedInfra.Reset()).MustNotHaveHappened();
            A.CallTo(() => mockedInfra.Destroy()).MustHaveHappenedOnceExactly();
        }
    }
}