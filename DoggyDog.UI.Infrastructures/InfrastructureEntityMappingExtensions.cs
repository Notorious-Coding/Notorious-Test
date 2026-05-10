using System.Text.Json;
using DoggyDog.UI.Context.Model;
using NotoriousTest.SqlLiteRegistry;

namespace DoggyDog.UI.Infrastructures;

public static class InfrastructureRegistryEntryEntityMappingExtensions
{
    extension(InfrastructureRegistryEntryEntity entry)
    {
        internal Infrastructure ToInfrastructure()
        {
            InfrastructureType infrastructureType =
                GetInfrastructureType(entry.MetadataType?.Split(',')[0].Split('.').Last());
            Type? metadataType = infrastructureType switch
            {
                InfrastructureType.Database => typeof(DatabaseMetadata),
                InfrastructureType.Docker => typeof(DockerMetadata),
                _ => null
            };

            return new Infrastructure(Id: Guid.Parse(entry.InfrastructureId),
                Name: entry.InfrastructureType.Split(',')[0].Split('.').Last(),
                infrastructureType,
                Metadata: entry.Metadata is null || metadataType is null
                    ? null
                    : JsonSerializer.Deserialize(entry.Metadata, metadataType),
                metadataType,
                ResetDate: entry.LastResetDate != null ? DateTime.Parse(entry.LastResetDate) : null,
                CreationDate:  DateTime.Parse(entry.CreationDate)
            );

        }

        private static InfrastructureType GetInfrastructureType(string? metadataType) =>
            metadataType switch
            {
                nameof(NotoriousTest.TestContainers.DockerMetadata) => InfrastructureType.Docker,
                nameof(NotoriousTest.Database.DatabaseMetadata) => InfrastructureType.Database,
                _ => InfrastructureType.None
            };
    }
}
