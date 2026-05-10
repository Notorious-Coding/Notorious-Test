using System.Collections.ObjectModel;
using DoggyDog.UI.Context.Adapters;
using Environment = DoggyDog.UI.Context.Model.Environment;

namespace DoggyDog.UI.Context;

public class EnvironmentContext : Context
{
    public delegate void OnEnvironmentListRefreshDelegate(bool hasChanged);
    public event OnEnvironmentListRefreshDelegate OnEnvironmentListRefresh;

    private readonly IRegistryRepository _repository;
    public ObservableCollection<Environment> Environments { get; private set; }
    public TimeOnly LastRefresh { get; set; }
    public bool IsMounted { get; set; }

    public EnvironmentContext(IRegistryRepository repository)
    {
        _repository = repository;
    }

    public async Task Update()
    {
        IEnumerable<Environment> environments = await _repository.GetAll();

        var toRemove = Environments.Except(environments).ToList();
        var toAdd = environments.Except(Environments).ToList();

        ExecuteOnUi(_ =>
        {
            foreach (Environment item in toRemove) Environments.Remove(item);
            foreach (Environment item in toAdd) Environments.Add(item);
            LastRefresh = TimeOnly.FromDateTime(DateTime.Now);
            OnEnvironmentListRefresh?.Invoke(hasChanged: toRemove.Any() || toAdd.Any());
        });
    }

    public override async Task OnMount()
    {
        if (!IsMounted)
        {
            Environments = new ObservableCollection<Environment>(await _repository.GetAll());

            _ = Task.Run(async () =>
            {
                while (true)
                {

                    await Update();
                    await Task.Delay(500);
                }
            });
            IsMounted = true;
        }
    }
}
