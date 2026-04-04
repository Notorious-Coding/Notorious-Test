using AwesomeAssertions;

using NotoriousTest.Core.Registry;
using NotoriousTest.SqlLiteRegistry;

using System.Text.Json;

// Note: InfrastructureRegistryEntryEntity is internal to NotoriousTest.SqlLiteRegistry.
// Add [assembly: InternalsVisibleTo("NotoriousTest.UnitTests")] to that project to enable these tests.

namespace NotoriousTest.UnitTests.Registry;

public class InfrastructureRegistryEntryEntityTests
{
    private record TestMetadata(string Name, int Value);

    [Fact]
    public void FromDomain_MapsAllFields()
    {
        var infrastructureId = Guid.NewGuid();
        var environmentId = Guid.NewGuid();
        var metadata = new TestMetadata("hello", 42);

        var entry = new InfrastuctureRegistryEntry
        {
            InfrastructureId = infrastructureId,
            InfrastructureType = typeof(InfrastuctureRegistryEntry),
            EnvironmentId = environmentId,
            ProcessID = 1234,
            Metadata = metadata,
        };

        var entity = InfrastructureRegistryEntryEntity.FromDomain(entry);

        entity!.InfrastructureId.Should().Be(infrastructureId.ToString());
        entity.InfrastructureType.Should().Be(typeof(InfrastuctureRegistryEntry).AssemblyQualifiedName);
        entity.EnvironmentId.Should().Be(environmentId.ToString());
        entity.ProcessID.Should().Be(1234);
        entity.Metadata.Should().Be(JsonSerializer.Serialize(metadata));
        entity.MetadataType.Should().Be(typeof(TestMetadata).AssemblyQualifiedName);
    }

    [Fact]
    public void FromDomain_NullMetadata_MetadataSerializedAsNullAndMetadataTypeIsNull()
    {
        var entry = new InfrastuctureRegistryEntry
        {
            InfrastructureId = Guid.NewGuid(),
            InfrastructureType = typeof(InfrastuctureRegistryEntry),
            EnvironmentId = Guid.NewGuid(),
            ProcessID = 1,
            Metadata = null,
        };

        var entity = InfrastructureRegistryEntryEntity.FromDomain(entry);

        entity!.Metadata.Should().Be("null");
        entity.MetadataType.Should().BeNull();
    }

    [Fact]
    public void ToDomain_MapsAllFields()
    {
        var infrastructureId = Guid.NewGuid();
        var environmentId = Guid.NewGuid();
        var creationDate = new DateTime(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc);
        var updateDate = new DateTime(2024, 2, 20, 12, 0, 0, DateTimeKind.Utc);

        var entity = new InfrastructureRegistryEntryEntity
        {
            InfrastructureId = infrastructureId.ToString(),
            InfrastructureType = typeof(InfrastuctureRegistryEntry).AssemblyQualifiedName,
            EnvironmentId = environmentId.ToString(),
            ProcessID = 5678,
            Metadata = null,
            MetadataType = null,
            CreationDate = creationDate.ToString("O"),
            UpdateDate = updateDate.ToString("O"),
        };

        var domain = entity.ToDomain();

        domain.InfrastructureId.Should().Be(infrastructureId);
        domain.InfrastructureType.Should().Be(typeof(InfrastuctureRegistryEntry));
        domain.EnvironmentId.Should().Be(environmentId);
        domain.ProcessID.Should().Be(5678);
        domain.Metadata.Should().BeNull();
        domain.CreationDate.Should().Be(creationDate);
        domain.UpdateDate.Should().Be(updateDate);
    }

    [Fact]
    public void ToDomain_UnknownInfrastructureType_ThrowsTypeLoadException()
    {
        var entity = new InfrastructureRegistryEntryEntity
        {
            InfrastructureId = Guid.NewGuid().ToString(),
            InfrastructureType = "NonExistent.Type.That.DoesNotExist, FakeAssembly",
            EnvironmentId = Guid.NewGuid().ToString(),
            ProcessID = 1,
            Metadata = null,
            MetadataType = null,
            CreationDate = DateTime.UtcNow.ToString("O"),
            UpdateDate = DateTime.UtcNow.ToString("O"),
        };

        var act = () => entity.ToDomain();

        act.Should().Throw<TypeLoadException>();
    }

    [Fact]
    public void ToDomain_RoundTrip_PreservesAllData()
    {
        var creationDate = new DateTime(2024, 3, 10, 8, 0, 0, DateTimeKind.Utc);
        var updateDate = new DateTime(2024, 4, 5, 16, 0, 0, DateTimeKind.Utc);

        var original = new InfrastuctureRegistryEntry
        {
            InfrastructureId = Guid.NewGuid(),
            InfrastructureType = typeof(InfrastuctureRegistryEntry),
            EnvironmentId = Guid.NewGuid(),
            ProcessID = 9999,
            Metadata = "some-metadata",
        };

        var entity = InfrastructureRegistryEntryEntity.FromDomain(original)!;
        // FromDomain does not map dates — they are set by the storage layer
        entity.CreationDate = creationDate.ToString("O");
        entity.UpdateDate = updateDate.ToString("O");

        var result = entity.ToDomain();

        result.InfrastructureId.Should().Be(original.InfrastructureId);
        result.InfrastructureType.Should().Be(original.InfrastructureType);
        result.EnvironmentId.Should().Be(original.EnvironmentId);
        result.ProcessID.Should().Be(original.ProcessID);
        result.Metadata.Should().Be(original.Metadata);
        result.CreationDate.Should().Be(creationDate);
        result.UpdateDate.Should().Be(updateDate);
    }
}
