using AwesomeAssertions;
using Fip.Strive.Tracking.Application.Features.Access;

namespace Fip.Strive.Tracking.Application.UnitTests.Access;

public class Pbkdf2PasswordTests
{
    [Fact]
    public void A_hash_verifies_against_the_password_it_was_made_from()
    {
        var encoded = Pbkdf2Password.Create("correct horse battery staple");

        Pbkdf2Password.Verify("correct horse battery staple", encoded).Should().BeTrue();
    }

    [Theory]
    [InlineData("Correct horse battery staple")]
    [InlineData("correct horse battery stapl")]
    [InlineData("")]
    [InlineData(null)]
    public void Anything_else_does_not(string? candidate)
    {
        var encoded = Pbkdf2Password.Create("correct horse battery staple");

        Pbkdf2Password.Verify(candidate, encoded).Should().BeFalse();
    }

    [Fact]
    public void The_same_password_hashes_differently_every_time()
    {
        var first = Pbkdf2Password.Create("hunter2");
        var second = Pbkdf2Password.Create("hunter2");

        first.Should().NotBe(second);
        Pbkdf2Password.Verify("hunter2", first).Should().BeTrue();
        Pbkdf2Password.Verify("hunter2", second).Should().BeTrue();
    }

    [Fact]
    public void The_encoding_carries_the_algorithm_and_its_cost()
    {
        var encoded = Pbkdf2Password.Create("hunter2");

        encoded.Split('$').Should().HaveCount(4);
        encoded.Should().StartWith("pbkdf2-sha256$210000$");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    [InlineData("hunter2")]
    [InlineData("pbkdf2-sha256$210000$onlythreeparts")]
    [InlineData("bcrypt$12$c2FsdA==$aGFzaA==")]
    [InlineData("pbkdf2-sha256$notanumber$c2FsdA==$aGFzaA==")]
    [InlineData("pbkdf2-sha256$0$c2FsdA==$aGFzaA==")]
    [InlineData("pbkdf2-sha256$210000$not base64$aGFzaA==")]
    [InlineData("pbkdf2-sha256$210000$c2FsdA==$")]
    public void A_malformed_hash_reads_as_a_wrong_password_rather_than_an_exception(string? encoded)
    {
        var verify = () => Pbkdf2Password.Verify("hunter2", encoded);

        verify.Should().NotThrow().Which.Should().BeFalse();
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("hunter2", false)]
    [InlineData("bcrypt$12$c2FsdA==$aGFzaA==", false)]
    [InlineData("pbkdf2-sha256$210000$c2FsdA==$aGFzaA==", true)]
    public void IsEncodedHash_recognises_the_shape_startup_validation_checks_for(
        string? encoded,
        bool expected
    ) => Pbkdf2Password.IsEncodedHash(encoded).Should().Be(expected);

    [Fact]
    public void Hashing_nothing_is_a_programming_error()
    {
        var create = () => Pbkdf2Password.Create("   ");

        create.Should().Throw<ArgumentException>();
    }
}
