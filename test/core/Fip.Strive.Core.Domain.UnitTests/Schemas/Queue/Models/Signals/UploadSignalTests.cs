using System.Text.Json;
using AwesomeAssertions;
using Fip.Strive.Core.Domain.Schemas.Queue.Enums;
using Fip.Strive.Core.Domain.Schemas.Queue.Models.Signals;

namespace Fip.Strive.Core.Domain.UnitTests.Schemas.Queue.Models.Signals;

public class UploadSignalTests
{
    [Fact]
    public void SerializingUploadSignal_WithBaseClass_ShouldSerializeFilepath()
    {
        var filepath = "/here/we/go";
        var hash = "hash1234";

        Signal signal = UploadSignal.From(filepath, hash);
        var job = signal.ToJobEntity();

        var uploadSignal = JsonSerializer.Deserialize<UploadSignal>(job.Payload!);

        uploadSignal.Should().NotBeNull();
        uploadSignal.Filepath.Should().Be(filepath);
    }

    [Fact]
    public void ToJobEntity_ShouldPreserveSignalMetadata()
    {
        var filepath = "/test/path";
        var hash = "hash1234";
        var signal = UploadSignal.From(filepath, hash);

        var job = signal.ToJobEntity();

        job.Id.Should().Be(signal.Id);
        job.Type.Should().Be(SignalTypes.UploadSignal);
        job.SignalledAt.Should().Be(signal.EmittedAt);
        job.Payload.Should().NotBeNull();
    }

    [Fact]
    public void ToJobEntity_ShouldRoundTripSuccessfully()
    {
        var filepath = "/another/test/path";
        var hash = "hash1234";
        var originalSignal = UploadSignal.From(filepath, hash);
        var originalId = originalSignal.Id;
        var originalReferenceId = originalSignal.ReferenceId;
        var originalEmittedAt = originalSignal.EmittedAt;

        var job = originalSignal.ToJobEntity();
        var deserializedSignal = JsonSerializer.Deserialize<UploadSignal>(job.Payload!);

        deserializedSignal.Should().NotBeNull();
        deserializedSignal.Filepath.Should().Be(filepath);
        deserializedSignal.Id.Should().Be(originalId);
        deserializedSignal.ReferenceId.Should().Be(originalReferenceId);
        deserializedSignal.EmittedAt.Should().Be(originalEmittedAt);
        deserializedSignal.Type.Should().Be(SignalTypes.UploadSignal);
    }
}
