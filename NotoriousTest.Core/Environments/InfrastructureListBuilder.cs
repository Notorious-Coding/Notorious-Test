using Microsoft.Extensions.DependencyInjection;
using NotoriousTest.Core.Infrastructures;

namespace NotoriousTest.Core.Environments;

public class InfrastructureListBuilder
{
    private readonly Dictionary<int, List<Func<IServiceProvider, EnvironmentId, Infrastructure>>> _infrastructures =
        new();


    public InfrastructureListBuilder Add<TInfrastructure>() where TInfrastructure : Infrastructure
        => Add(CreateWithDependencyInjection<TInfrastructure>);

    public InfrastructureListBuilder Add(Infrastructure infrastructure)
        => Add((_, _) => infrastructure);

    public InfrastructureListBuilder ThenAdd<TInfrastructure>() where TInfrastructure : Infrastructure
        => ThenAdd(CreateWithDependencyInjection<TInfrastructure>);

    public InfrastructureListBuilder ThenAdd(Infrastructure infrastructure)
        => ThenAdd((_, _) => infrastructure);

    public Dictionary<int, List<Infrastructure>> Build(IServiceProvider provider, EnvironmentId environmentId,
        bool disableWatchdog)
    {
        Dictionary<int, List<Infrastructure>> infrastructures = new();
        foreach ((int order, List<Func<IServiceProvider, EnvironmentId, Infrastructure>> factories) in _infrastructures)
            infrastructures.Add(order,
                factories.Select(factory =>
                {
                    Infrastructure infrastructure = factory(provider, environmentId);
                    infrastructure.EnvironmentId = environmentId;
                    infrastructure.WatchdogDisabled = disableWatchdog;
                    return infrastructure;
                }).ToList());

        return infrastructures;
    }

    private InfrastructureListBuilder Add(Func<IServiceProvider, EnvironmentId, Infrastructure> factory)
    {
        if (_infrastructures.Count == 0)
            _infrastructures.Add(0, []);

        _infrastructures[_infrastructures.Keys.Max()].Add(factory);
        return this;
    }

    private InfrastructureListBuilder ThenAdd(Func<IServiceProvider, EnvironmentId, Infrastructure> factory)
    {
        int nextGroup = _infrastructures.Count == 0 ? 0 : _infrastructures.Keys.Max() + 1;
        _infrastructures.Add(nextGroup, [factory]);
        return this;
    }

    private static Infrastructure CreateWithDependencyInjection<TInfrastructure>(IServiceProvider provider,
        EnvironmentId environmentId) where TInfrastructure : Infrastructure
        => ActivatorUtilities.CreateInstance<TInfrastructure>(provider, environmentId);
}
