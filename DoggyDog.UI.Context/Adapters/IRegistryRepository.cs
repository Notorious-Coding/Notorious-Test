using DoggyDog.UI.Context.Model;

namespace DoggyDog.UI.Context.Adapters;

public interface IRegistryRepository
{
    Task<IEnumerable<Process>> GetAll();
}
