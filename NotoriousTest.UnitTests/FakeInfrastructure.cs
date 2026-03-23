using FakeItEasy;


using NotoriousTest.Infrastructures;
using NotoriousTest.Logger;

namespace NotoriousTest.UnitTests;

public abstract class FakeInfrastructure : Infrastructure
{
    protected FakeInfrastructure() : base(Guid.NewGuid(), A.Fake<ITestLogger>()) { }
}
