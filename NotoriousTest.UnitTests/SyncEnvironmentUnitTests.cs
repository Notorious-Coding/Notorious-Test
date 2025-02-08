using NotoriousTest.Common.Environments;
using NotoriousTest.Common.Infrastructures.Sync;
using Environment = NotoriousTest.Common.Environments.Environment;
using FakeItEasy;
using NotoriousTest.Common.Infrastructures.Async;
namespace NotoriousTest.UnitTests
{

    public class SyncEnvironmentUnitTests
    {
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
    }
}