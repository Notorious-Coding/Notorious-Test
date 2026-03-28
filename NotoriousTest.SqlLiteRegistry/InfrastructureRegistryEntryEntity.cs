using NotoriousTest.Core.Registry;

using System;
using System.Text.Json;

namespace NotoriousTest.SqlLiteRegistry
{
    internal class InfrastructureRegistryEntryEntity
    {
        public string InfrastructureId { get; set; }
        public string InfrastructureType { get; set; }
        public string EnvironmentId { get; set; }
        public int ProcessID { get; set; }
        public string? Metadata { get; set; }
        public string? MetadataType { get; set; }
        public string CreationDate { get; set; }
        public string UpdateDate { get; set; }

        internal static InfrastructureRegistryEntryEntity? FromDomain(InfrastuctureRegistryEntry entry)
        {
            return new InfrastructureRegistryEntryEntity()
            {
                InfrastructureId = entry.InfrastructureId.ToString(),
                InfrastructureType = entry.InfrastructureType.AssemblyQualifiedName,
                EnvironmentId = entry.EnvironmentId.ToString(),
                ProcessID = entry.ProcessID,
                Metadata = JsonSerializer.Serialize(entry.Metadata),
                MetadataType = entry.Metadata?.GetType().AssemblyQualifiedName
            };
        }

        internal InfrastuctureRegistryEntry ToDomain()
        {
            Type? metadataType = null;

            if (MetadataType != null)
            {
                metadataType = Type.GetType(MetadataType);
            }

            Type? infraType = Type.GetType(InfrastructureType);
            if (infraType == null)
            {
                throw new TypeLoadException($"Unable to load type {InfrastructureType}");
            }

            return new InfrastuctureRegistryEntry()
            {
                InfrastructureId = Guid.Parse(InfrastructureId),
                InfrastructureType = Type.GetType(InfrastructureType),
                EnvironmentId = Guid.Parse(EnvironmentId),
                ProcessID = ProcessID,
                Metadata = !(Metadata is null) && !(metadataType is null)
                    ? JsonSerializer.Deserialize(Metadata, metadataType)
                    : null,
                CreationDate = DateTime.Parse(CreationDate),
                UpdateDate = DateTime.Parse(UpdateDate),
            };
        }
    }
}
