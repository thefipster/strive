using AwesomeAssertions;
using Fip.Strive.Harvester.Application.Features.Import.Components.Contracts;
using Fip.Strive.Harvester.Application.Features.Import.Components.Decorators;
using NSubstitute;

namespace Fip.Strive.Harvester.Application.UnitTests.Features.Import.Components.Decorators;

public class ZipFileAccessValidatorTests
{
    private readonly IZipFileAccess _component;
    private readonly ZipFileAccessValidator _sut;
    private readonly string _testRootPath;

    public ZipFileAccessValidatorTests()
    {
        _component = Substitute.For<IZipFileAccess>();
        _sut = new ZipFileAccessValidator(_component);
        _testRootPath = Path.Combine(
            Path.GetTempPath(),
            $"ZipFileAccessValidatorTests_{Guid.NewGuid()}"
        );
    }

    [Fact]
    public void Import_WhenPathIsValid_ShouldCallComponentImport()
    {
        // Arrange
        var uploadPath = Path.Combine(_testRootPath, "test.zip");
        Directory.CreateDirectory(_testRootPath);
        File.WriteAllText(uploadPath, "test content");
        var expectedResult = "/import/test.zip";

        _component.Import(uploadPath).Returns(expectedResult);

        try
        {
            // Act
            var result = _sut.Import(uploadPath);

            // Assert
            result.Should().Be(expectedResult);
            _component.Received(1).Import(uploadPath);
        }
        finally
        {
            if (Directory.Exists(_testRootPath))
                Directory.Delete(_testRootPath, true);
        }
    }

    [Fact]
    public void Import_WhenPathIsNull_ShouldThrowArgumentException()
    {
        // Arrange
        string? uploadPath = null;

        // Act
        var act = () => _sut.Import(uploadPath!);

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("*path cannot be null or empty*");
        _component.DidNotReceive().Import(Arg.Any<string>());
    }

    [Fact]
    public void Import_WhenPathIsEmpty_ShouldThrowArgumentException()
    {
        // Arrange
        var uploadPath = string.Empty;

        // Act
        var act = () => _sut.Import(uploadPath);

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("*path cannot be null or empty*");
        _component.DidNotReceive().Import(Arg.Any<string>());
    }

    [Fact]
    public void Import_WhenPathIsWhitespace_ShouldThrowArgumentException()
    {
        // Arrange
        var uploadPath = "   ";

        // Act
        var act = () => _sut.Import(uploadPath);

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("*path cannot be null or empty*");
        _component.DidNotReceive().Import(Arg.Any<string>());
    }

    [Fact]
    public void Import_WhenFileDoesNotExist_ShouldThrowFileNotFoundException()
    {
        // Arrange
        var uploadPath = Path.Combine(_testRootPath, "nonexistent.zip");

        // Act
        var act = () => _sut.Import(uploadPath);

        // Assert
        act.Should().Throw<FileNotFoundException>().WithMessage("*file does not exist*");
        _component.DidNotReceive().Import(Arg.Any<string>());
    }

    [Fact]
    public void Open_WhenPathIsValid_ShouldCallComponentOpen()
    {
        // Arrange
        var importPath = Path.Combine(_testRootPath, "imported.zip");
        Directory.CreateDirectory(_testRootPath);
        File.WriteAllText(importPath, "test content");
        var expectedStream = new FileStream(
            importPath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read
        );

        _component.Open(importPath).Returns(expectedStream);

        try
        {
            // Act
            var result = _sut.Open(importPath);

            // Assert
            result.Should().BeSameAs(expectedStream);
            _component.Received(1).Open(importPath);
        }
        finally
        {
            expectedStream.Dispose();
            if (Directory.Exists(_testRootPath))
                Directory.Delete(_testRootPath, true);
        }
    }

    [Fact]
    public void Open_WhenPathIsNull_ShouldThrowArgumentException()
    {
        // Arrange
        string? importPath = null;

        // Act
        var act = () => _sut.Open(importPath!);

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("*path cannot be null or empty*");
        _component.DidNotReceive().Open(Arg.Any<string>());
    }

    [Fact]
    public void Open_WhenPathIsEmpty_ShouldThrowArgumentException()
    {
        // Arrange
        var importPath = string.Empty;

        // Act
        var act = () => _sut.Open(importPath);

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("*path cannot be null or empty*");
        _component.DidNotReceive().Open(Arg.Any<string>());
    }

    [Fact]
    public void Open_WhenPathIsWhitespace_ShouldThrowArgumentException()
    {
        // Arrange
        var importPath = "   ";

        // Act
        var act = () => _sut.Open(importPath);

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("*path cannot be null or empty*");
        _component.DidNotReceive().Open(Arg.Any<string>());
    }

    [Fact]
    public void Open_WhenFileDoesNotExist_ShouldThrowFileNotFoundException()
    {
        // Arrange
        var importPath = Path.Combine(_testRootPath, "nonexistent.zip");

        // Act
        var act = () => _sut.Open(importPath);

        // Assert
        act.Should().Throw<FileNotFoundException>().WithMessage("*file does not exist*");
        _component.DidNotReceive().Open(Arg.Any<string>());
    }
}
