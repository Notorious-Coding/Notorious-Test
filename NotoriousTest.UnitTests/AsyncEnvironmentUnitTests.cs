using FakeItEasy;
using NotoriousTest.Common.Environments;
using NotoriousTest.Common.Infrastructures;
using NotoriousTest.Common.Infrastructures.Async;

namespace NotoriousTest.UnitTests
{

    public class AsyncEnvironmentUnitTests
    {
        #region Unique Infrastructure Test Cases
        #region Unique Infrastructure Test Cases Setup
        static UniqueInfrastructureTestCasesInfrastructure _uniqueInfrastructureTestCasesInfrastructure;

        public class UniqueInfrastructureTestCasesEnvironment : AsyncEnvironment
        {

            public UniqueInfrastructureTestCasesEnvironment()
            {

            }

            public override Task ConfigureEnvironmentAsync()
            {
                AddInfrastructure(_uniqueInfrastructureTestCasesInfrastructure);

                return Task.CompletedTask;
            }
        }

        public abstract class UniqueInfrastructureTestCasesInfrastructure : AsyncInfrastructure
        {
            public UniqueInfrastructureTestCasesInfrastructure() : base(false)
            {

            }

        }
        #endregion

        [Fact]
        public async Task GetInfrastructure_Should_ReturnProperInfrastructure()
        {
            _uniqueInfrastructureTestCasesInfrastructure = A.Fake<UniqueInfrastructureTestCasesInfrastructure>();
            var environment = new UniqueInfrastructureTestCasesEnvironment();
            await environment.InitializeAsync();
            UniqueInfrastructureTestCasesInfrastructure infra = await environment.GetInfrastructureAsync<UniqueInfrastructureTestCasesInfrastructure>();

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
        #region Multiple Infrastructure Test Cases Setup
        static MultipleInfrastructureTestCasesInfrastructure1 _multipleInfrastructureTestCasesInfrastructure1;
        static MultipleInfrastructureTestCasesInfrastructure2 _multipleInfrastructureTestCasesInfrastructure2;

        public class MultipleInfrastructureTestCasesEnvironment : AsyncEnvironment
        {

            public MultipleInfrastructureTestCasesEnvironment()
            {

            }

            public override Task ConfigureEnvironmentAsync()
            {
                AddInfrastructure(_multipleInfrastructureTestCasesInfrastructure1);
                AddInfrastructure(_multipleInfrastructureTestCasesInfrastructure2);

                return Task.CompletedTask;
            }
        }

        public abstract class MultipleInfrastructureTestCasesInfrastructure1 : AsyncInfrastructure
        {

            public MultipleInfrastructureTestCasesInfrastructure1() : base(false)
            {

            }

        }

        public abstract class MultipleInfrastructureTestCasesInfrastructure2 : AsyncInfrastructure
        {

            public MultipleInfrastructureTestCasesInfrastructure2() : base(false)
            {

            }
        }
        #endregion

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
        #region Configuration Object Test Cases Setup
        static ConfigurationObjectTestCasesInfrastructure1 _configurationObjectTestCasesInfrastructure1;
        static ConfigurationObjectTestCasesInfrastructure2 _configurationObjectTestCasesInfrastructure2;

        public class ConfigurationObjectTestCasesEnvironmentConfiguraton
        {
            public string Key1 { get; set; }
            public string Key2 { get; set; }
        }

        public class ConfigurationObjectTestCasesEnvironment : AsyncConfiguredEnvironment<ConfigurationObjectTestCasesEnvironmentConfiguraton>
        {
            public ConfigurationObjectTestCasesEnvironment()
            {
            }
            public override Task ConfigureEnvironmentAsync()
            {
                AddInfrastructure(_configurationObjectTestCasesInfrastructure1);
                AddInfrastructure(_configurationObjectTestCasesInfrastructure2);
                return Task.CompletedTask;
            }
        }

        public class ConfigurationObjectTestCasesInfrastructure1 : AsyncConfiguredInfrastructure<ConfigurationObjectTestCasesEnvironmentConfiguraton>
        {
            public ConfigurationObjectTestCasesInfrastructure1() : base(false)
            {
            }

            public override Task Destroy()
            {
                return Task.CompletedTask;
            }

            public override Task Initialize()
            {
                Configuration.Key1 = "Infra1Key1";

                return Task.CompletedTask;
            }

            public override Task Reset()
            {
                return Task.CompletedTask;
            }
        }

        public class ConfigurationObjectTestCasesInfrastructure2 : AsyncConfiguredInfrastructure<ConfigurationObjectTestCasesEnvironmentConfiguraton>
        {
            public ConfigurationObjectTestCasesInfrastructure2() : base(false)
            {
            }

            public override Task Destroy()
            {
                return Task.CompletedTask;
            }

            public override Task Initialize()
            {
                Configuration.Key2 = "Infra2Key2";

                return Task.CompletedTask;
            }

            public override Task Reset()
            {
                return Task.CompletedTask;
            }
        }


        #endregion

        [Fact]
        public async Task InfrastructureWithinEnvironmentWithObjectConfiguration_Should_ProduceConfigurationCorrectly()
        {
            _configurationObjectTestCasesInfrastructure1 = new ConfigurationObjectTestCasesInfrastructure1();
            _configurationObjectTestCasesInfrastructure2 = new ConfigurationObjectTestCasesInfrastructure2();

            var environment = new ConfigurationObjectTestCasesEnvironment();
            await environment.InitializeAsync();
            Assert.Equal("Infra1Key1", environment.EnvironmentConfiguration.Key1);
            Assert.Equal("Infra2Key2", environment.EnvironmentConfiguration.Key2);
        }

        #endregion
        #region Configuration Dictionary Test Cases
        #region Configuration Dictionary Test Cases Setup
        static ConfigurationDictionaryTestCasesInfrastructure1 _configurationDictionaryTestCasesInfrastructure1;
        static ConfigurationDictionaryTestCasesInfrastructure2 _configurationDictionaryTestCasesInfrastructure2;

        public class ConfigurationDictionaryTestCasesEnvironment : AsyncConfiguredEnvironment
        {
            public ConfigurationDictionaryTestCasesEnvironment()
            {
            }
            public override Task ConfigureEnvironmentAsync()
            {
                AddInfrastructure(_configurationDictionaryTestCasesInfrastructure1);
                AddInfrastructure(_configurationDictionaryTestCasesInfrastructure2);
                return Task.CompletedTask;
            }
        }

        public class ConfigurationDictionaryTestCasesInfrastructure1 : AsyncConfiguredInfrastructure
        {
            public ConfigurationDictionaryTestCasesInfrastructure1() : base(false)
            {
            }

            public override Task Destroy()
            {
                return Task.CompletedTask;
            }

            public override Task Initialize()
            {
                Configuration.Add("Key1", "Infra1Key1");

                return Task.CompletedTask;
            }

            public override Task Reset()
            {
                return Task.CompletedTask;
            }
        }

        public class ConfigurationDictionaryTestCasesInfrastructure2 : AsyncConfiguredInfrastructure
        {
            public ConfigurationDictionaryTestCasesInfrastructure2() : base(false)
            {
            }

            public override Task Destroy()
            {
                return Task.CompletedTask;
            }

            public override Task Initialize()
            {
                Configuration.Add("Key2", "Infra2Key2");

                return Task.CompletedTask;
            }

            public override Task Reset()
            {
                return Task.CompletedTask;
            }
        }


        #endregion

        [Fact]
        public async Task InfrastructureWithinEnvironmentWithDictionaryConfiguration_Should_ProduceConfigurationCorrectly()
        {
            _configurationDictionaryTestCasesInfrastructure1 = new ConfigurationDictionaryTestCasesInfrastructure1();
            _configurationDictionaryTestCasesInfrastructure2 = new ConfigurationDictionaryTestCasesInfrastructure2();

            var environment = new ConfigurationDictionaryTestCasesEnvironment();
            await environment.InitializeAsync();
            Assert.Equal("Infra1Key1", environment.EnvironmentConfiguration["Key1"]);
            Assert.Equal("Infra2Key2", environment.EnvironmentConfiguration["Key2"]);
        }

        #endregion
    }
}