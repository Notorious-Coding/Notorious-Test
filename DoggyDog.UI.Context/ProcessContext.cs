using System.Collections.ObjectModel;
using DoggyDog.UI.Context.Adapters;
using DoggyDog.UI.Context.Model;

namespace DoggyDog.UI.Context;

public class ProcessContext : Context
{
    public delegate void OnProcessListRefreshDelegate(bool hasChanged);

    private readonly IRegistryRepository _repository;

    public ProcessContext(IRegistryRepository repository)
    {
        _repository = repository;
    }

    public ObservableCollection<Process> Processes { get; private set; }
    public TimeOnly LastRefresh { get; set; }
    public bool IsMounted { get; set; }
    public event OnProcessListRefreshDelegate OnProcessListRefresh;

    public async Task Update()
    {
        IEnumerable<Process> processes = await _repository.GetAll();

        var toRemove = Processes.Except(processes).ToList();
        var toAdd = processes.Except(Processes).ToList();

        ExecuteOnUi(_ =>
        {
            foreach (Process item in toRemove) Processes.Remove(item);
            foreach (Process item in toAdd) Processes.Add(item);
            LastRefresh = TimeOnly.FromDateTime(DateTime.Now);
            OnProcessListRefresh?.Invoke(toRemove.Any() || toAdd.Any());
        });
    }

    public override async Task OnMount()
    {
        if (!IsMounted)
        {
            Processes = new ObservableCollection<Process>(await _repository.GetAll());

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
