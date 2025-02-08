using NotoriousTest.Common.Environments;
using NotoriousTest.Common.Infrastructures.Sync;
using Environment = NotoriousTest.Common.Environments.Environment;
using FakeItEasy;
using NotoriousTest.Common.Infrastructures.Async;
namespace NotoriousTest.UnitTests
{

    public class SyncEnvironmentUnitTests
    {
        #region Unique Infrastructure Test Cases
        #region Unique Infrastructure Test Cases Setup
        static UniqueInfrastructureTestCasesInfrastructure _uniqueInfrastructureTestCasesInfrastructure;

        public class UniqueInfrastructureTestCasesEnvironment : Environment
        {
            public UniqueInfrastructureTestCasesEnvironment()
            {
                
            }

            public override void ConfigureEnvironment()
            {
                AddInfrastructure(_uniqueInfrastructureTestCasesInfrastructure);
            }
        }   

        public abstract class UniqueInfrastructureTestCasesInfrastructure : Infrastructure
        {

            public UniqueInfrastructureTestCasesInfrastructure() : base(false)
            {
                
            }
        }
        #endregion

        [Fact]
        public void GetInfrastructure_Should_ReturnProperInfrastructure()
        {
            _uniqueInfrastructureTestCasesInfrastructure = A.Fake<UniqueInfrastructureTestCasesInfrastructure>();
            var environment = new UniqueInfrastructureTestCasesEnvironment();

            UniqueInfrastructureTestCasesInfrastructure infra = environment.GetInfrastructure<UniqueInfrastructureTestCasesInfrastructure>();

            Assert.NotNull(infra);
            Assert.Equal(environment.EnvironmentId, infra.ContextId);
        }

        [Fact]
        public void EnvironmentCreation_Should_CallInfrastructureInitialization()
        {
            _uniqueInfrastructureTestCasesInfrastructure = A.Fake<UniqueInfrastructureTestCasesInfrastructure>();

            var environment = new UniqueInfrastructureTestCasesEnvironment();

            A.CallTo(() => _uniqueInfrastructureTestCasesInfrastructure.Initialize()).MustHaveHappenedOnceExactly();
            A.CallTo(() => _uniqueInfrastructureTestCasesInfrastructure.Reset()).MustNotHaveHappened();
            A.CallTo(() => _uniqueInfrastructureTestCasesInfrastructure.Destroy()).MustNotHaveHappened();
        }

        [Fact]
        public void Reset_Should_CallInfrastructureReset()
        {
            _uniqueInfrastructureTestCasesInfrastructure = A.Fake<UniqueInfrastructureTestCasesInfrastructure>();

            var environment = new UniqueInfrastructureTestCasesEnvironment();

            environment.Reset();

            A.CallTo(() => _uniqueInfrastructureTestCasesInfrastructure.Initialize()).MustHaveHappenedOnceExactly();
            A.CallTo(() => _uniqueInfrastructureTestCasesInfrastructure.Reset()).MustHaveHappenedOnceExactly();
            A.CallTo(() => _uniqueInfrastructureTestCasesInfrastructure.Destroy()).MustNotHaveHappened();
        }

        [Fact]
        public void Destroy_Should_CallInfrastructureDestroy()
        {
            _uniqueInfrastructureTestCasesInfrastructure = A.Fake<UniqueInfrastructureTestCasesInfrastructure>();

            var environment = new UniqueInfrastructureTestCasesEnvironment();

            environment.Destroy();

            A.CallTo(() => _uniqueInfrastructureTestCasesInfrastructure.Initialize()).MustHaveHappenedOnceExactly();
            A.CallTo(() => _uniqueInfrastructureTestCasesInfrastructure.Reset()).MustNotHaveHappened();
            A.CallTo(() => _uniqueInfrastructureTestCasesInfrastructure.Destroy()).MustHaveHappenedOnceExactly();
        }
        #endregion
        #region Multiple Infrastructure Test Cases
        #region Multiple Infrastructure Test Cases Setup
        static MultipleInfrastructureTestCasesInfrastructure1 _multipleInfrastructureTestCasesInfrastructure1;
        static MultipleInfrastructureTestCasesInfrastructure2 _multipleInfrastructureTestCasesInfrastructure2;

        public class MultipleInfrastructureTestCasesEnvironment : Environment
        {

            public MultipleInfrastructureTestCasesEnvironment()
            {

            }

            public override void ConfigureEnvironment()
            {
                AddInfrastructure(_multipleInfrastructureTestCasesInfrastructure1);
                AddInfrastructure(_multipleInfrastructureTestCasesInfrastructure2);
            }
        }

        public abstract class MultipleInfrastructureTestCasesInfrastructure1 : Infrastructure
        {

            public MultipleInfrastructureTestCasesInfrastructure1() : base(false)
            {

            }

        }

        public abstract class MultipleInfrastructureTestCasesInfrastructure2 : Infrastructure
        {

            public MultipleInfrastructureTestCasesInfrastructure2() : base(false)
            {

            }
        }
        #endregion

        [Fact]
        public void Initialize_Should_CallInfrastructureInitializeInOrder()
        {
            _multipleInfrastructureTestCasesInfrastructure1 = A.Fake<MultipleInfrastructureTestCasesInfrastructure1>();
            _multipleInfrastructureTestCasesInfrastructure2 = A.Fake<MultipleInfrastructureTestCasesInfrastructure2>();

            A.CallTo(() => _multipleInfrastructureTestCasesInfrastructure1.Order).Returns(1);
            A.CallTo(() => _multipleInfrastructureTestCasesInfrastructure2.Order).Returns(2);

            var environment = new MultipleInfrastructureTestCasesEnvironment();

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
        public void Initialize_Should_CallInfrastructureResetInOrder()
        {
            _multipleInfrastructureTestCasesInfrastructure1 = A.Fake<MultipleInfrastructureTestCasesInfrastructure1>();
            _multipleInfrastructureTestCasesInfrastructure2 = A.Fake<MultipleInfrastructureTestCasesInfrastructure2>();

            A.CallTo(() => _multipleInfrastructureTestCasesInfrastructure1.Order).Returns(1);
            A.CallTo(() => _multipleInfrastructureTestCasesInfrastructure2.Order).Returns(2);

            var environment = new MultipleInfrastructureTestCasesEnvironment();

            environment.Reset();

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
        public void Initialize_Should_CallInfrastructureDestroyInOrder()
        {
            _multipleInfrastructureTestCasesInfrastructure1 = A.Fake<MultipleInfrastructureTestCasesInfrastructure1>();
            _multipleInfrastructureTestCasesInfrastructure2 = A.Fake<MultipleInfrastructureTestCasesInfrastructure2>();

            A.CallTo(() => _multipleInfrastructureTestCasesInfrastructure1.Order).Returns(1);
            A.CallTo(() => _multipleInfrastructureTestCasesInfrastructure2.Order).Returns(2);

            var environment = new MultipleInfrastructureTestCasesEnvironment();

            environment.Destroy();

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

        public class ConfigurationObjectTestCasesEnvironment : ConfiguredEnvironment<ConfigurationObjectTestCasesEnvironmentConfiguraton>
        {
            public ConfigurationObjectTestCasesEnvironment()
            {
            }
            public override void ConfigureEnvironment()
            {
                AddInfrastructure(_configurationObjectTestCasesInfrastructure1);
                AddInfrastructure(_configurationObjectTestCasesInfrastructure2);
            }
        }

        public class ConfigurationObjectTestCasesInfrastructure1 : ConfiguredInfrastructure<ConfigurationObjectTestCasesEnvironmentConfiguraton>
        {
            public ConfigurationObjectTestCasesInfrastructure1() : base(false)
            {
            }

            public override void Destroy()
            {
            }

            public override void Initialize()
            {
                Configuration.Key1 = "Infra1Key1";
            }

            public override void Reset()
            {
            }
        }

        public class ConfigurationObjectTestCasesInfrastructure2 : ConfiguredInfrastructure<ConfigurationObjectTestCasesEnvironmentConfiguraton>
        {
            public ConfigurationObjectTestCasesInfrastructure2() : base(false)
            {
            }

            public override void Destroy()
            {
            }

            public override void Initialize()
            {
                Configuration.Key2 = "Infra2Key2";
            }

            public override void Reset()
            {
            }
        }


        #endregion

        [Fact]
        public void InfrastructureWithinEnvironmentWithObjectConfiguration_Should_ProduceConfigurationCorrectly()
        {
            _configurationObjectTestCasesInfrastructure1 = new ConfigurationObjectTestCasesInfrastructure1();
            _configurationObjectTestCasesInfrastructure2 = new ConfigurationObjectTestCasesInfrastructure2();

            var environment = new ConfigurationObjectTestCasesEnvironment();
            Assert.Equal("Infra1Key1", environment.EnvironmentConfiguration.Key1);
            Assert.Equal("Infra2Key2", environment.EnvironmentConfiguration.Key2);
        }

        #endregion
        #region Configuration Dictionary Test Cases
        #region Configuration Dictionary Test Cases Setup
        static ConfigurationDictionaryTestCasesInfrastructure1 _configurationDictionaryTestCasesInfrastructure1;
        static ConfigurationDictionaryTestCasesInfrastructure2 _configurationDictionaryTestCasesInfrastructure2;

        public class ConfigurationDictionaryTestCasesEnvironment : ConfiguredEnvironment
        {
            public ConfigurationDictionaryTestCasesEnvironment()
            {
            }
            public override void ConfigureEnvironment()
            {
                AddInfrastructure(_configurationDictionaryTestCasesInfrastructure1);
                AddInfrastructure(_configurationDictionaryTestCasesInfrastructure2);
            }
        }

        public class ConfigurationDictionaryTestCasesInfrastructure1 : ConfiguredInfrastructure
        {
            public ConfigurationDictionaryTestCasesInfrastructure1() : base(false)
            {
            }

            public override void Destroy()
            {
            }

            public override void Initialize()
            {
                Configuration.Add("Key1", "Infra1Key1");
            }

            public override void Reset()
            {
            }
        }

        public class ConfigurationDictionaryTestCasesInfrastructure2 : ConfiguredInfrastructure
        {
            public ConfigurationDictionaryTestCasesInfrastructure2() : base(false)
            {
            }

            public override void Destroy()
            {
            }

            public override void Initialize()
            {
                Configuration.Add("Key2", "Infra2Key2");
            }

            public override void Reset()
            {
            }
        }


        #endregion

        [Fact]
        public void InfrastructureWithinEnvironmentWithDictionaryConfiguration_Should_ProduceConfigurationCorrectly()
        {
            _configurationDictionaryTestCasesInfrastructure1 = new ConfigurationDictionaryTestCasesInfrastructure1();
            _configurationDictionaryTestCasesInfrastructure2 = new ConfigurationDictionaryTestCasesInfrastructure2();

            var environment = new ConfigurationDictionaryTestCasesEnvironment();
            Assert.Equal("Infra1Key1", environment.EnvironmentConfiguration["Key1"]);
            Assert.Equal("Infra2Key2", environment.EnvironmentConfiguration["Key2"]);
        }

        #endregion
    }
}