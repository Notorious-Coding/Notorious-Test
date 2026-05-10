namespace DoggyDog.UI.Context.Adapters;

public interface IRegistryRepository
{
    Task<IEnumerable<Model.Environment>> GetAll();
}
