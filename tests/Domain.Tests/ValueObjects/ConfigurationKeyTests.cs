using Domain.ValueObjects;
using FluentAssertions;

namespace Domain.Tests.ValueObjects;

public class ConfigurationKeyTests
{
    [Fact]
    public void Create_WithValidKey_ShouldSucceed()
    {
        // Arrange
        var validKey = "Database.ConnectionString";

        // Act
        var result = ConfigurationKey.Create(validKey);

        // Assert
        result.Value.Should().Be(validKey);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_WithEmptyKey_ShouldThrowArgumentException(string invalidKey)
    {
        // Act & Assert
        var action = () => ConfigurationKey.Create(invalidKey);
        action.Should().Throw<ArgumentException>()
            .WithMessage("*Configuration key cannot be empty*");
    }

    [Fact]
    public void Create_WithNullKey_ShouldThrowArgumentException()
    {
        // Act & Assert
        var action = () => ConfigurationKey.Create(null!);
        action.Should().Throw<ArgumentException>()
            .WithMessage("*Configuration key cannot be null*");
    }

    [Fact]
    public void Create_WithKeyTooLong_ShouldThrowArgumentException()
    {
        // Arrange
        var longKey = new string('a', 201); // Exceeds 200 character limit

        // Act & Assert
        var action = () => ConfigurationKey.Create(longKey);
        action.Should().Throw<ArgumentException>()
            .WithMessage("*Configuration key cannot exceed 200 characters*");
    }

    [Theory]
    [InlineData("1InvalidKey")] // Starts with number
    [InlineData("Invalid Key")] // Contains space
    [InlineData("Invalid@Key")] // Contains invalid character
    [InlineData("Invalid#Key")] // Contains invalid character
    public void Create_WithInvalidFormat_ShouldThrowArgumentException(string invalidKey)
    {
        // Act & Assert
        var action = () => ConfigurationKey.Create(invalidKey);
        action.Should().Throw<ArgumentException>()
            .WithMessage("*Configuration key must start with a letter*");
    }

    [Theory]
    [InlineData("ValidKey")]
    [InlineData("Valid_Key")]
    [InlineData("Valid-Key")]
    [InlineData("Valid.Key")]
    [InlineData("Valid123")]
    [InlineData("Database.Connection.String")]
    public void Create_WithValidFormats_ShouldSucceed(string validKey)
    {
        // Act
        var result = ConfigurationKey.Create(validKey);

        // Assert
        result.Value.Should().Be(validKey);
    }

    [Fact]
    public void ImplicitConversion_ToString_ShouldWork()
    {
        // Arrange
        var key = ConfigurationKey.Create("TestKey");

        // Act
        string stringValue = key;

        // Assert
        stringValue.Should().Be("TestKey");
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        // Arrange
        var key = ConfigurationKey.Create("TestKey");

        // Act
        var result = key.ToString();

        // Assert
        result.Should().Be("TestKey");
    }
}
