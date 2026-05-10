namespace DoggyDog.UI.Context.Model;

public record Infrastructure(Guid Id, string Name, InfrastructureType Type, object? Metadata, Type? MetadataType, DateTime? ResetDate, DateTime CreationDate)
{

    public T? GetMetadata<T>() where T : class => Metadata is not T t ? null : t;
}

public enum InfrastructureType {
    None,
    Docker,
    Database
}
