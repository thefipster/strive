using System.Text.Json;
using AwesomeAssertions;
using Fip.Strive.Core.Domain.Schemas.Queue.Enums;
using Fip.Strive.Core.Domain.Schemas.Queue.Models.Signals;

namespace Fip.Strive.Core.Domain.UnitTests.Schemas.Queue.Models.Signals;

public class ImportSignalTests
{
    [Fact]
    public void SerializingImportSignal_WithBaseClass_ShouldSerializeFilepath()
    {
        var filepath = "/import/path";
        var uploadSignal = UploadSignal.From("/upload/path", "hash1234");

        Signal signal = ImportSignal.From(filepath, uploadSignal);
        var job = signal.ToJobEntity();

        var importSignal = JsonSerializer.Deserialize<ImportSignal>(job.Payload!);

        importSignal.Should().NotBeNull();
        importSignal.Filepath.Should().Be(filepath);
    }

    [Fact]
    public void ToJobEntity_ShouldPreserveSignalMetadata()
    {
        var filepath = "/test/import";
        var uploadSignal = UploadSignal.From("/test/upload", "hash1234");
        var signal = ImportSignal.From(filepath, uploadSignal);

        var job = signal.ToJobEntity();

        job.Id.Should().Be(signal.Id);
        job.Type.Should().Be(SignalTypes.ImportSignal);
        job.SignalledAt.Should().Be(signal.EmittedAt);
        job.Payload.Should().NotBeNull();
    }

    [Fact]
    public void ToJobEntity_ShouldRoundTripSuccessfully()
    {
        var filepath = "/another/import/path";
        var uploadSignal = UploadSignal.From("/another/upload/path", "hash1234");
        var originalSignal = ImportSignal.From(filepath, uploadSignal);
        var originalId = originalSignal.Id;
        var originalReferenceId = originalSignal.ReferenceId;
        var originalEmittedAt = originalSignal.EmittedAt;

        var job = originalSignal.ToJobEntity();
        var deserializedSignal = JsonSerializer.Deserialize<ImportSignal>(job.Payload!);

        deserializedSignal.Should().NotBeNull();
        deserializedSignal.Filepath.Should().Be(filepath);
        deserializedSignal.Id.Should().Be(originalId);
        deserializedSignal.ReferenceId.Should().Be(originalReferenceId);
        deserializedSignal.EmittedAt.Should().Be(originalEmittedAt);
        deserializedSignal.Type.Should().Be(SignalTypes.ImportSignal);
    }

    [Fact]
    public void From_ShouldInheritReferenceIdFromOriginSignal()
    {
        var uploadSignal = UploadSignal.From("/upload/path", "hash1234");
        var importSignal = ImportSignal.From("/import/path", uploadSignal);

        importSignal.ReferenceId.Should().Be(uploadSignal.ReferenceId);
    }
}
