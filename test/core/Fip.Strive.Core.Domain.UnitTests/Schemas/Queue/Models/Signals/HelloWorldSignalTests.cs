using System.Text.Json;
using AwesomeAssertions;
using Fip.Strive.Core.Domain.Schemas.Queue.Enums;
using Fip.Strive.Core.Domain.Schemas.Queue.Models.Signals;

namespace Fip.Strive.Core.Domain.UnitTests.Schemas.Queue.Models.Signals;

public class HelloWorldSignalTests
{
    [Fact]
    public void SerializingHelloWorldSignal_WithBaseClass_ShouldSerializeDiceRoll()
    {
        Signal signal = new HelloWorldSignal();
        var diceRoll = ((HelloWorldSignal)signal).DiceRoll;
        var job = signal.ToJobEntity();

        var helloWorldSignal = JsonSerializer.Deserialize<HelloWorldSignal>(job.Payload!);

        helloWorldSignal.Should().NotBeNull();
        helloWorldSignal.DiceRoll.Should().Be(diceRoll);
    }

    [Fact]
    public void ToJobEntity_ShouldPreserveSignalMetadata()
    {
        var signal = new HelloWorldSignal();

        var job = signal.ToJobEntity();

        job.Id.Should().Be(signal.Id);
        job.Type.Should().Be(SignalTypes.HelloWorldSignal);
        job.SignalledAt.Should().Be(signal.EmittedAt);
        job.Payload.Should().NotBeNull();
    }

    [Fact]
    public void ToJobEntity_ShouldRoundTripSuccessfully()
    {
        var originalSignal = new HelloWorldSignal();
        var originalId = originalSignal.Id;
        var originalReferenceId = originalSignal.ReferenceId;
        var originalEmittedAt = originalSignal.EmittedAt;
        var originalDiceRoll = originalSignal.DiceRoll;

        var job = originalSignal.ToJobEntity();
        var deserializedSignal = JsonSerializer.Deserialize<HelloWorldSignal>(job.Payload!);

        deserializedSignal.Should().NotBeNull();
        deserializedSignal.DiceRoll.Should().Be(originalDiceRoll);
        deserializedSignal.Id.Should().Be(originalId);
        deserializedSignal.ReferenceId.Should().Be(originalReferenceId);
        deserializedSignal.EmittedAt.Should().Be(originalEmittedAt);
        deserializedSignal.Type.Should().Be(SignalTypes.HelloWorldSignal);
    }

    [Fact]
    public void DiceRoll_ShouldBeInValidRange()
    {
        var signal = new HelloWorldSignal();

        signal.DiceRoll.Should().BeInRange(1, 20);
    }
}
