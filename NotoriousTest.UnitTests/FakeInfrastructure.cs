using NotoriousTest.Infrastructures;

namespace NotoriousTest.UnitTests;

public abstract class FakeInfrastructure : Infrastructure
{
    protected FakeInfrastructure() : base(Guid.NewGuid()) { }
}
