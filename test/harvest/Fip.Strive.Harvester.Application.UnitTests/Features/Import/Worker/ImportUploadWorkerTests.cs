using System.Text.Json;
using AwesomeAssertions;
using Fip.Strive.Core.Domain.Schemas.Queue.Enums;
using Fip.Strive.Core.Domain.Schemas.Queue.Models;
using Fip.Strive.Core.Domain.Schemas.Queue.Models.Signals;
using Fip.Strive.Harvester.Application.Core.Queue.Exceptions;
using Fip.Strive.Harvester.Application.Features.Import.Models;
using Fip.Strive.Harvester.Application.Features.Import.Services.Contracts;
using Fip.Strive.Harvester.Application.Features.Import.Worker;
using NSubstitute;

namespace Fip.Strive.Harvester.Application.UnitTests.Features.Import.Worker;

public class ImportUploadWorkerTests
{
    private readonly IZipInventory _inventory;
    private readonly ImportUploadWorker _sut;

    public ImportUploadWorkerTests()
    {
        _inventory = Substitute.For<IZipInventory>();
        _sut = new ImportUploadWorker(_inventory);
    }

    [Fact]
    public void Type_ShouldReturnUploadSignal()
    {
        // Arrange
        // Act
        var result = _sut.Type;

        // Assert
        result.Should().Be(SignalTypes.UploadSignal);
    }

    [Fact]
    public async Task ProcessAsync_WithValidPayload_ShouldCallInventoryImportAsync()
    {
        // Arrange
        var uploadSignal = UploadSignal.From(@"C:\upload\test.zip", "some-hash");
        var payload = JsonSerializer.Serialize(uploadSignal);
        var job = new JobDetails
        {
            Id = Guid.NewGuid(),
            Type = SignalTypes.UploadSignal,
            Payload = payload,
            SignalledAt = DateTime.UtcNow,
        };
        var cancellationToken = new CancellationToken();

        var work = WorkItem.FromSignal(uploadSignal);
        work.ImportedPath = "test.zip";
        _inventory.ImportAsync(Arg.Any<UploadSignal>(), cancellationToken).Returns(work);

        // Act
        await _sut.ProcessAsync(job, cancellationToken);

        // Assert
        await _inventory
            .Received(1)
            .ImportAsync(
                Arg.Is<UploadSignal>(s => s.Filepath == uploadSignal.Filepath),
                cancellationToken
            );
    }

    [Fact]
    public async Task ProcessAsync_ShouldDeserializeSignalCorrectly()
    {
        // Arrange
        var expectedFilepath = @"C:\upload\myarchive.zip";
        var expectedHash = "hash123";
        var uploadSignal = UploadSignal.From(expectedFilepath, expectedHash);
        var payload = JsonSerializer.Serialize(uploadSignal);
        var job = new JobDetails
        {
            Id = Guid.NewGuid(),
            Type = SignalTypes.UploadSignal,
            Payload = payload,
            SignalledAt = DateTime.UtcNow,
        };

        var work = WorkItem.FromSignal(uploadSignal);
        _inventory.ImportAsync(Arg.Any<UploadSignal>(), Arg.Any<CancellationToken>()).Returns(work);

        // Act
        await _sut.ProcessAsync(job, CancellationToken.None);

        // Assert
        await _inventory
            .Received(1)
            .ImportAsync(
                Arg.Is<UploadSignal>(s =>
                    s.Filepath == expectedFilepath
                    && s.Hash == expectedHash
                    && s.Id == uploadSignal.Id
                    && s.ReferenceId == uploadSignal.ReferenceId
                ),
                Arg.Any<CancellationToken>()
            );
    }

    [Fact]
    public async Task ProcessAsync_WhenPayloadIsNull_ShouldThrowInvalidJobException()
    {
        // Arrange
        var job = new JobDetails
        {
            Id = Guid.NewGuid(),
            Type = SignalTypes.UploadSignal,
            Payload = null,
            SignalledAt = DateTime.UtcNow,
        };

        // Act
        var act = async () => await _sut.ProcessAsync(job, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidJobException>().WithMessage("*Payload is null*");
        await _inventory
            .DidNotReceive()
            .ImportAsync(Arg.Any<UploadSignal>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ProcessAsync_WhenPayloadIsEmpty_ShouldThrowInvalidJobException()
    {
        // Arrange
        var job = new JobDetails
        {
            Id = Guid.NewGuid(),
            Type = SignalTypes.UploadSignal,
            Payload = string.Empty,
            SignalledAt = DateTime.UtcNow,
        };

        // Act
        var act = async () => await _sut.ProcessAsync(job, CancellationToken.None);

        // Assert
        await act.Should()
            .ThrowAsync<JsonException>()
            .WithMessage("*The input does not contain any JSON tokens.*");
        await _inventory
            .DidNotReceive()
            .ImportAsync(Arg.Any<UploadSignal>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ProcessAsync_WhenPayloadIsInvalid_ShouldThrowInvalidJobException()
    {
        // Arrange
        var job = new JobDetails
        {
            Id = Guid.NewGuid(),
            Type = SignalTypes.UploadSignal,
            Payload = "invalid json {{{",
            SignalledAt = DateTime.UtcNow,
        };

        // Act
        var act = async () => await _sut.ProcessAsync(job, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<JsonException>();
        await _inventory
            .DidNotReceive()
            .ImportAsync(Arg.Any<UploadSignal>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ProcessAsync_WhenPayloadDeserializesToNull_ShouldThrowInvalidJobException()
    {
        // Arrange
        var job = new JobDetails
        {
            Id = Guid.NewGuid(),
            Type = SignalTypes.UploadSignal,
            Payload = "null",
            SignalledAt = DateTime.UtcNow,
        };

        // Act
        var act = async () => await _sut.ProcessAsync(job, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidJobException>().WithMessage("*Can't read payload*");
        await _inventory
            .DidNotReceive()
            .ImportAsync(Arg.Any<UploadSignal>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ProcessAsync_ShouldPassCancellationTokenToInventory()
    {
        // Arrange
        var uploadSignal = UploadSignal.From(@"C:\upload\test.zip", "some-hash");
        var payload = JsonSerializer.Serialize(uploadSignal);
        var job = new JobDetails
        {
            Id = Guid.NewGuid(),
            Type = SignalTypes.UploadSignal,
            Payload = payload,
            SignalledAt = DateTime.UtcNow,
        };
        var cancellationToken = new CancellationToken();

        var work = WorkItem.FromSignal(uploadSignal);
        _inventory.ImportAsync(Arg.Any<UploadSignal>(), cancellationToken).Returns(work);

        // Act
        await _sut.ProcessAsync(job, cancellationToken);

        // Assert
        await _inventory.Received(1).ImportAsync(Arg.Any<UploadSignal>(), cancellationToken);
    }

    [Fact]
    public async Task ProcessAsync_WithComplexUploadSignal_ShouldPreserveAllProperties()
    {
        // Arrange
        var uploadSignal = UploadSignal.From(@"C:\complex\path\to\file.zip", "some-hash");
        var originalId = uploadSignal.Id;
        var originalReferenceId = uploadSignal.ReferenceId;
        var originalEmittedAt = uploadSignal.EmittedAt;

        var payload = JsonSerializer.Serialize(uploadSignal);
        var job = new JobDetails
        {
            Id = Guid.NewGuid(),
            Type = SignalTypes.UploadSignal,
            Payload = payload,
            SignalledAt = DateTime.UtcNow,
        };

        var work = WorkItem.FromSignal(uploadSignal);
        _inventory.ImportAsync(Arg.Any<UploadSignal>(), Arg.Any<CancellationToken>()).Returns(work);

        // Act
        await _sut.ProcessAsync(job, CancellationToken.None);

        // Assert
        await _inventory
            .Received(1)
            .ImportAsync(
                Arg.Is<UploadSignal>(s =>
                    s.Id == originalId
                    && s.ReferenceId == originalReferenceId
                    && s.EmittedAt == originalEmittedAt
                    && s.Filepath == @"C:\complex\path\to\file.zip"
                ),
                Arg.Any<CancellationToken>()
            );
    }
}
