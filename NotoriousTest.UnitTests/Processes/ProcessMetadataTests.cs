using System.Text.Json;
using AwesomeAssertions;
using NotoriousTest.Processes;

namespace NotoriousTest.UnitTests.Processes;

public class ProcessMetadataTests
{
    [Fact]
    public void ProcessMetadata_Should_RoundTrip_Through_Registry_Json_Serialization()
    {
        var metadata = new ProcessMetadata
        {
            ProcessId = 4242,
            StartTimeUtc = new DateTime(2026, 11, 4, 10, 30, 15, 123, DateTimeKind.Utc)
        };

        // Same calls as the SQLite registry: serialized as object, deserialized from its assembly qualified name.
        string json = JsonSerializer.Serialize((object)metadata);
        Type? metadataType = Type.GetType(metadata.GetType().AssemblyQualifiedName!);
        object? deserialized = JsonSerializer.Deserialize(json, metadataType!);

        deserialized.Should().BeOfType<ProcessMetadata>().Which.Should().BeEquivalentTo(metadata);
        ((ProcessMetadata)deserialized!).StartTimeUtc.Kind.Should().Be(DateTimeKind.Utc);
    }
}
