namespace DoggyDog.UI.Context.Model;

public record Environment(Guid Id, IEnumerable<Infrastructure> Infrastructures)
{
    public override string ToString() => $"Environment {Id.ToString()[..8]}";
}
