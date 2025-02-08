using NotoriousTest.Common.Environments;
using NotoriousTest.Common.Infrastructures.Sync;
using Environment = NotoriousTest.Common.Environments.Environment;
using FakeItEasy;
namespace NotoriousTest.UnitTests
{

    public class SyncEnvironmentUnitTests
    {

        static TestInfrastructure mockedInfra;

        public class TestEnvironment : Environment
        {
            public TestEnvironment()
            {
                
            }

            public override void ConfigureEnvironment()
            {
                AddInfrastructure(mockedInfra);
            }
        }   

        public abstract class TestInfrastructure : Infrastructure
        {

            public TestInfrastructure() : base(false)
            {
                
            }
        }


        [Fact]
        public void GetInfrastructure_Should_ReturnProperInfrastructure()
        {
            mockedInfra = A.Fake<TestInfrastructure>();
            var environment = new TestEnvironment();

            TestInfrastructure infra = environment.GetInfrastructure<TestInfrastructure>();

            Assert.NotNull(infra);
            Assert.Equal(environment.EnvironmentId, infra.ContextId);
        }

        [Fact]
        public void EnvironmentCreation_Should_CallInfrastructureInitialization()
        {
            mockedInfra = A.Fake<TestInfrastructure>();

            var environment = new TestEnvironment();

            A.CallTo(() => mockedInfra.Initialize()).MustHaveHappenedOnceExactly();
            A.CallTo(() => mockedInfra.Reset()).MustNotHaveHappened();
            A.CallTo(() => mockedInfra.Destroy()).MustNotHaveHappened();
        }

        [Fact]
        public void Reset_Should_CallInfrastructureReset()
        {
            mockedInfra = A.Fake<TestInfrastructure>();

            var environment = new TestEnvironment();

            environment.Reset();

            A.CallTo(() => mockedInfra.Initialize()).MustHaveHappenedOnceExactly();
            A.CallTo(() => mockedInfra.Reset()).MustHaveHappenedOnceExactly();
            A.CallTo(() => mockedInfra.Destroy()).MustNotHaveHappened();
        }

        [Fact]
        public void Destroy_Should_CallInfrastructureDestroy()
        {
            mockedInfra = A.Fake<TestInfrastructure>();

            var environment = new TestEnvironment();

            environment.Destroy();

            A.CallTo(() => mockedInfra.Initialize()).MustHaveHappenedOnceExactly();
            A.CallTo(() => mockedInfra.Reset()).MustNotHaveHappened();
            A.CallTo(() => mockedInfra.Destroy()).MustHaveHappenedOnceExactly();
        }
    }
}