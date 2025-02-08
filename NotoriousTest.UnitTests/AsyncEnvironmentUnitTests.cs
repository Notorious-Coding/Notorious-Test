using FakeItEasy;
using NotoriousTest.Common.Environments;
using NotoriousTest.Common.Infrastructures.Async;

namespace NotoriousTest.UnitTests
{

    public class AsyncEnvironmentUnitTests
    {
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
    }
}