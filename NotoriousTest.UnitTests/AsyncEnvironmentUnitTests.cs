using FakeItEasy;

namespace NotoriousTest.UnitTests
{

    public partial class AsyncEnvironmentUnitTests
    {
        #region Unique Infrastructure Test Cases
        static UniqueInfrastructureTestCasesInfrastructure _uniqueInfrastructureTestCasesInfrastructure;

        [Fact]
        public async Task GetInfrastructure_Should_ReturnProperInfrastructure()
        {
            _uniqueInfrastructureTestCasesInfrastructure = A.Fake<UniqueInfrastructureTestCasesInfrastructure>();
            var environment = new UniqueInfrastructureTestCasesEnvironment();
            await environment.InitializeAsync();
            UniqueInfrastructureTestCasesInfrastructure infra = environment.GetInfrastructure<UniqueInfrastructureTestCasesInfrastructure>();

            Assert.NotNull(infra);
            Assert.Equal(environment.EnvironmentId, infra.ContextId);
        }

        [Fact]
        public async Task EnvironmentCreation_Should_CallInfrastructureInitialization()
        {
            _uniqueInfrastructureTestCasesInfrastructure = A.Fake<UniqueInfrastructureTestCasesInfrastructure>();

            var environment = new UniqueInfrastructureTestCasesEnvironment();

            await environment.InitializeAsync();

            A.CallTo(() => _uniqueInfrastructureTestCasesInfrastructure.Initialize()).MustHaveHappenedOnceExactly();
            A.CallTo(() => _uniqueInfrastructureTestCasesInfrastructure.Reset()).MustNotHaveHappened();
            A.CallTo(() => _uniqueInfrastructureTestCasesInfrastructure.Destroy()).MustNotHaveHappened();
        }

        [Fact]
        public async Task Reset_Should_CallInfrastructureReset()
        {
            _uniqueInfrastructureTestCasesInfrastructure = A.Fake<UniqueInfrastructureTestCasesInfrastructure>();

            var environment = new UniqueInfrastructureTestCasesEnvironment();

            await environment.InitializeAsync();
            await environment.Reset();

            A.CallTo(() => _uniqueInfrastructureTestCasesInfrastructure.Initialize()).MustHaveHappenedOnceExactly();
            A.CallTo(() => _uniqueInfrastructureTestCasesInfrastructure.Reset()).MustHaveHappenedOnceExactly();
            A.CallTo(() => _uniqueInfrastructureTestCasesInfrastructure.Destroy()).MustNotHaveHappened();
        }

        [Fact]
        public async Task Destroy_Should_CallInfrastructureDestroy()
        {
            _uniqueInfrastructureTestCasesInfrastructure = A.Fake<UniqueInfrastructureTestCasesInfrastructure>();

            var environment = new UniqueInfrastructureTestCasesEnvironment();

            await environment.InitializeAsync();
            await environment.Destroy();

            A.CallTo(() => _uniqueInfrastructureTestCasesInfrastructure.Initialize()).MustHaveHappenedOnceExactly();
            A.CallTo(() => _uniqueInfrastructureTestCasesInfrastructure.Reset()).MustNotHaveHappened();
            A.CallTo(() => _uniqueInfrastructureTestCasesInfrastructure.Destroy()).MustHaveHappenedOnceExactly();

        }
        #endregion
        #region Multiple Infrastructures Test Cases
        static MultipleInfrastructureTestCasesInfrastructure1 _multipleInfrastructureTestCasesInfrastructure1;
        static MultipleInfrastructureTestCasesInfrastructure2 _multipleInfrastructureTestCasesInfrastructure2;

        [Fact]
        public async Task Initialize_Should_CallInfrastructuresInOrder()
        {
            _multipleInfrastructureTestCasesInfrastructure1 = A.Fake<MultipleInfrastructureTestCasesInfrastructure1>();
            _multipleInfrastructureTestCasesInfrastructure2 = A.Fake<MultipleInfrastructureTestCasesInfrastructure2>();

            A.CallTo(() => _multipleInfrastructureTestCasesInfrastructure1.Order).Returns(1);
            A.CallTo(() => _multipleInfrastructureTestCasesInfrastructure2.Order).Returns(2);

            var environment = new MultipleInfrastructureTestCasesEnvironment();

            await environment.InitializeAsync();

            A.CallTo(() => _multipleInfrastructureTestCasesInfrastructure1.Initialize())
                .MustHaveHappenedOnceExactly()
                .Then(
                    A.CallTo(() => _multipleInfrastructureTestCasesInfrastructure2.Initialize())
                        .MustHaveHappenedOnceExactly()
                );

            A.CallTo(() => _multipleInfrastructureTestCasesInfrastructure1.Reset()).MustNotHaveHappened();
            A.CallTo(() => _multipleInfrastructureTestCasesInfrastructure1.Destroy()).MustNotHaveHappened();

            A.CallTo(() => _multipleInfrastructureTestCasesInfrastructure2.Reset()).MustNotHaveHappened();
            A.CallTo(() => _multipleInfrastructureTestCasesInfrastructure2.Destroy()).MustNotHaveHappened();
        }

        [Fact]
        public async Task Initialize_Should_CallInfrastructureResetInOrder()
        {
            _multipleInfrastructureTestCasesInfrastructure1 = A.Fake<MultipleInfrastructureTestCasesInfrastructure1>();
            _multipleInfrastructureTestCasesInfrastructure2 = A.Fake<MultipleInfrastructureTestCasesInfrastructure2>();

            A.CallTo(() => _multipleInfrastructureTestCasesInfrastructure1.Order).Returns(1);
            A.CallTo(() => _multipleInfrastructureTestCasesInfrastructure2.Order).Returns(2);

            var environment = new MultipleInfrastructureTestCasesEnvironment();

            await environment.InitializeAsync();
            await environment.Reset();

            A.CallTo(() => _multipleInfrastructureTestCasesInfrastructure1.Initialize())
                .MustHaveHappenedOnceExactly()
                .Then(
                    A.CallTo(() => _multipleInfrastructureTestCasesInfrastructure2.Initialize())
                        .MustHaveHappenedOnceExactly()
                );

            A.CallTo(() => _multipleInfrastructureTestCasesInfrastructure1.Reset())
                .MustHaveHappenedOnceExactly()
                .Then(
                    A.CallTo(() => _multipleInfrastructureTestCasesInfrastructure2.Reset())
                        .MustHaveHappenedOnceExactly()
                );

            A.CallTo(() => _multipleInfrastructureTestCasesInfrastructure1.Destroy()).MustNotHaveHappened();
            A.CallTo(() => _multipleInfrastructureTestCasesInfrastructure2.Destroy()).MustNotHaveHappened();
        }

        [Fact]
        public async Task Initialize_Should_CallInfrastructureDestroyInOrder()
        {
            _multipleInfrastructureTestCasesInfrastructure1 = A.Fake<MultipleInfrastructureTestCasesInfrastructure1>();
            _multipleInfrastructureTestCasesInfrastructure2 = A.Fake<MultipleInfrastructureTestCasesInfrastructure2>();

            A.CallTo(() => _multipleInfrastructureTestCasesInfrastructure1.Order).Returns(1);
            A.CallTo(() => _multipleInfrastructureTestCasesInfrastructure2.Order).Returns(2);

            var environment = new MultipleInfrastructureTestCasesEnvironment();

            await environment.InitializeAsync();
            await environment.Destroy();

            A.CallTo(() => _multipleInfrastructureTestCasesInfrastructure1.Initialize())
                .MustHaveHappenedOnceExactly()
                .Then(
                    A.CallTo(() => _multipleInfrastructureTestCasesInfrastructure2.Initialize())
                        .MustHaveHappenedOnceExactly()
                );

            A.CallTo(() => _multipleInfrastructureTestCasesInfrastructure1.Destroy())
                .MustHaveHappenedOnceExactly()
                .Then(
                    A.CallTo(() => _multipleInfrastructureTestCasesInfrastructure2.Destroy())
                        .MustHaveHappenedOnceExactly()
                );
        }

        #endregion
        #region Configuration Object Test Cases
        static ConfigurationObjectTestCasesInfrastructure1 _configurationObjectTestCasesInfrastructure1;
        static ConfigurationObjectTestCasesInfrastructureWithSection2 _configurationObjectTestCasesInfrastructure2;

        public class ConfigurationObjectTestCasesInfrastructureConfiguraton1
        {
            public string Key1 { get; set; }
        }

        public class ConfigurationObjectTestCasesInfrastructureConfiguraton2
        {
            public string Key2 { get; set; }
        }

        [Fact]
        public async Task InfrastructureWithinEnvironmentWithObjectConfiguration_Should_ProduceConfigurationCorrectly()
        {
            _configurationObjectTestCasesInfrastructure1 = new ConfigurationObjectTestCasesInfrastructure1();
            _configurationObjectTestCasesInfrastructure2 = new ConfigurationObjectTestCasesInfrastructureWithSection2();

            var environment = new ConfigurationObjectTestCasesEnvironment();
            await environment.InitializeAsync();
            var config1 = environment.OutputConfiguration.FirstOrDefault(ce => ce.Key == "ConfigurationObjectTestCasesInfrastructureConfiguraton1")?.Value as ConfigurationObjectTestCasesInfrastructureConfiguraton1;
            var config2 = environment.OutputConfiguration.FirstOrDefault(ce => ce.Key == "Toto")?.Value as ConfigurationObjectTestCasesInfrastructureConfiguraton2;
            Assert.Equal("Infra1Key1", config1?.Key1);
            Assert.Equal("Infra2Key2", config2?.Key2);
        }

        #endregion
        #region Configuration Dictionary Test Cases
        static ConfigurationDictionaryTestCasesInfrastructure1 _configurationDictionaryTestCasesInfrastructure1;
        static ConfigurationDictionaryTestCasesInfrastructure2 _configurationDictionaryTestCasesInfrastructure2;

        [Fact]
        public async Task InfrastructureWithinEnvironmentWithDictionaryConfiguration_Should_ProduceConfigurationCorrectly()
        {
            _configurationDictionaryTestCasesInfrastructure1 = new ConfigurationDictionaryTestCasesInfrastructure1();
            _configurationDictionaryTestCasesInfrastructure2 = new ConfigurationDictionaryTestCasesInfrastructure2();

            var environment = new ConfigurationDictionaryTestCasesEnvironment();
            await environment.InitializeAsync();

            var config1 = environment.OutputConfiguration.FirstOrDefault(ce => ce.Key == "ConfigurationDictionaryTestCasesInfrastructure1:Key1")?.Value as string;
            var config2 = environment.OutputConfiguration.FirstOrDefault(ce => ce.Key == "ConfigurationDictionaryTestCasesInfrastructure2:Key2")?.Value as string;
            Assert.Equal("Infra1Key1", config1);
            Assert.Equal("Infra2Key2", config2);
        }

        #endregion
    }
}