using FakeItEasy;

namespace NotoriousTest.UnitTests
{
    public partial class AsyncEnvironmentUnitTests
    {
        #region Unique Infrastructure Test Cases

        [Fact]
        public async Task GetInfrastructure_Should_ReturnProperInfrastructure()
        {
            var infra = A.Fake<UniqueInfrastructureTestCasesInfrastructure>();
            var environment = new UniqueInfrastructureTestCasesEnvironment(infra);
            await environment.InitializeAsync();

            var result = environment.GetInfrastructure<UniqueInfrastructureTestCasesInfrastructure>();

            Assert.NotNull(result);
            Assert.Equal(environment.EnvironmentId, result.ContextId);
        }

        [Fact]
        public async Task EnvironmentCreation_Should_CallInfrastructureInitialization()
        {
            var infra = A.Fake<UniqueInfrastructureTestCasesInfrastructure>();
            var environment = new UniqueInfrastructureTestCasesEnvironment(infra);

            await environment.InitializeAsync();

            A.CallTo(() => infra.Initialize()).MustHaveHappenedOnceExactly();
            A.CallTo(() => infra.Reset()).MustNotHaveHappened();
            A.CallTo(() => infra.Destroy()).MustNotHaveHappened();
        }

        [Fact]
        public async Task Reset_Should_CallInfrastructureReset()
        {
            var infra = A.Fake<UniqueInfrastructureTestCasesInfrastructure>();
            var environment = new UniqueInfrastructureTestCasesEnvironment(infra);

            await environment.InitializeAsync();
            await environment.Reset();

            A.CallTo(() => infra.Initialize()).MustHaveHappenedOnceExactly();
            A.CallTo(() => infra.Reset()).MustHaveHappenedOnceExactly();
            A.CallTo(() => infra.Destroy()).MustNotHaveHappened();
        }

        [Fact]
        public async Task Destroy_Should_CallInfrastructureDestroy()
        {
            var infra = A.Fake<UniqueInfrastructureTestCasesInfrastructure>();
            var environment = new UniqueInfrastructureTestCasesEnvironment(infra);

            await environment.InitializeAsync();
            await environment.Destroy();

            A.CallTo(() => infra.Initialize()).MustHaveHappenedOnceExactly();
            A.CallTo(() => infra.Reset()).MustNotHaveHappened();
            A.CallTo(() => infra.Destroy()).MustHaveHappenedOnceExactly();
        }

        #endregion

        #region Multiple Infrastructures Test Cases

        [Fact]
        public async Task Initialize_Should_CallInfrastructuresInOrder()
        {
            var infra1 = A.Fake<MultipleInfrastructureTestCasesInfrastructure1>();
            var infra2 = A.Fake<MultipleInfrastructureTestCasesInfrastructure2>();
            A.CallTo(() => infra1.Order).Returns(1);
            A.CallTo(() => infra2.Order).Returns(2);

            var environment = new MultipleInfrastructureTestCasesEnvironment(infra1, infra2);
            await environment.InitializeAsync();

            A.CallTo(() => infra1.Initialize())
                .MustHaveHappenedOnceExactly()
                .Then(A.CallTo(() => infra2.Initialize()).MustHaveHappenedOnceExactly());

            A.CallTo(() => infra1.Reset()).MustNotHaveHappened();
            A.CallTo(() => infra1.Destroy()).MustNotHaveHappened();
            A.CallTo(() => infra2.Reset()).MustNotHaveHappened();
            A.CallTo(() => infra2.Destroy()).MustNotHaveHappened();
        }

        [Fact]
        public async Task Initialize_Should_CallInfrastructureResetInOrder()
        {
            var infra1 = A.Fake<MultipleInfrastructureTestCasesInfrastructure1>();
            var infra2 = A.Fake<MultipleInfrastructureTestCasesInfrastructure2>();
            A.CallTo(() => infra1.Order).Returns(1);
            A.CallTo(() => infra2.Order).Returns(2);

            var environment = new MultipleInfrastructureTestCasesEnvironment(infra1, infra2);
            await environment.InitializeAsync();
            await environment.Reset();

            A.CallTo(() => infra1.Initialize())
                .MustHaveHappenedOnceExactly()
                .Then(A.CallTo(() => infra2.Initialize()).MustHaveHappenedOnceExactly());

            A.CallTo(() => infra1.Reset())
                .MustHaveHappenedOnceExactly()
                .Then(A.CallTo(() => infra2.Reset()).MustHaveHappenedOnceExactly());

            A.CallTo(() => infra1.Destroy()).MustNotHaveHappened();
            A.CallTo(() => infra2.Destroy()).MustNotHaveHappened();
        }

        [Fact]
        public async Task Initialize_Should_CallInfrastructureDestroyInOrder()
        {
            var infra1 = A.Fake<MultipleInfrastructureTestCasesInfrastructure1>();
            var infra2 = A.Fake<MultipleInfrastructureTestCasesInfrastructure2>();
            A.CallTo(() => infra1.Order).Returns(1);
            A.CallTo(() => infra2.Order).Returns(2);

            var environment = new MultipleInfrastructureTestCasesEnvironment(infra1, infra2);
            await environment.InitializeAsync();
            await environment.Destroy();

            A.CallTo(() => infra1.Initialize())
                .MustHaveHappenedOnceExactly()
                .Then(A.CallTo(() => infra2.Initialize()).MustHaveHappenedOnceExactly());

            A.CallTo(() => infra1.Destroy())
                .MustHaveHappenedOnceExactly()
                .Then(A.CallTo(() => infra2.Destroy()).MustHaveHappenedOnceExactly());
        }

        #endregion

        #region Configuration Object Test Cases

        public class ConfigurationObjectTestCasesInfrastructureConfiguration1
        {
            public string Key1 { get; set; }
        }

        public class ConfigurationObjectTestCasesInfrastructureConfiguration2
        {
            public string Key2 { get; set; }
        }

        [Fact]
        public async Task InfrastructureWithinEnvironmentWithObjectConfiguration_Should_ProduceConfigurationCorrectly()
        {
            var infra1 = new ConfigurationObjectTestCasesInfrastructure1();
            var infra2 = new ConfigurationObjectTestCasesInfrastructureWithSection2();
            var environment = new ConfigurationObjectTestCasesEnvironment(infra1, infra2);

            await environment.InitializeAsync();

            var config1 = infra1.OutputConfiguration.FirstOrDefault(ce => ce.Key == nameof(ConfigurationObjectTestCasesInfrastructureConfiguration1))?.Value;
            var config2 = infra2.OutputConfiguration.FirstOrDefault(ce => ce.Key == "Toto")?.Value;
            Assert.Equal("Infra1Key1", config1?.Key1);
            Assert.Equal("Infra2Key2", config2?.Key2);
        }

        #endregion

        #region Configuration Dictionary Test Cases

        [Fact]
        public async Task InfrastructureWithinEnvironmentWithDictionaryConfiguration_Should_ProduceConfigurationCorrectly()
        {
            var infra1 = new ConfigurationDictionaryTestCasesInfrastructure1();
            var infra2 = new ConfigurationDictionaryTestCasesInfrastructure2();
            var environment = new ConfigurationDictionaryTestCasesEnvironment(infra1, infra2);

            await environment.InitializeAsync();

            var config1 = infra1.OutputConfiguration.FirstOrDefault(ce => ce.Key == "ConfigurationDictionaryTestCasesInfrastructure1:Key1")?.Value;
            var config2 = infra2.OutputConfiguration.FirstOrDefault(ce => ce.Key == "ConfigurationDictionaryTestCasesInfrastructure2:Key2")?.Value;
            Assert.Equal("Infra1Key1", config1);
            Assert.Equal("Infra2Key2", config2);
        }

        #endregion
    }
}
