using Domain.ValueObjects;
using FluentAssertions;

namespace Domain.Tests.ValueObjects;

public class ConfigurationValueTests
{
    [Theory]
    [InlineData("test string", ConfigurationValueType.String)]
    [InlineData("123", ConfigurationValueType.Integer)]
    [InlineData("true", ConfigurationValueType.Boolean)]
    [InlineData("123.45", ConfigurationValueType.Decimal)]
    [InlineData("2023-12-01", ConfigurationValueType.DateTime)]
    [InlineData("{\"key\":\"value\"}", ConfigurationValueType.Json)]
    public void Create_WithValidValueAndType_ShouldSucceed(string value, ConfigurationValueType type)
    {
        // Act
        var result = ConfigurationValue.Create(value, type);

        // Assert
        result.Value.Should().Be(value);
        result.Type.Should().Be(type);
    }

    [Fact]
    public void Create_WithNullValue_ShouldThrowArgumentException()
    {
        // Act & Assert
        var action = () => ConfigurationValue.Create(null!, ConfigurationValueType.String);
        action.Should().Throw<ArgumentException>()
            .WithMessage("*Configuration value cannot be null*");
    }

    [Theory]
    [InlineData("not a number", ConfigurationValueType.Integer)]
    [InlineData("not a boolean", ConfigurationValueType.Boolean)]
    [InlineData("not a decimal", ConfigurationValueType.Decimal)]
    [InlineData("not a date", ConfigurationValueType.DateTime)]
    [InlineData("invalid json {", ConfigurationValueType.Json)]
    public void Create_WithInvalidValueForType_ShouldThrowArgumentException(string value, ConfigurationValueType type)
    {
        // Act & Assert
        var action = () => ConfigurationValue.Create(value, type);
        action.Should().Throw<ArgumentException>()
            .WithMessage($"*Value '{value}' is not valid for type {type}*");
    }

    [Fact]
    public void GetTypedValue_WithStringType_ShouldReturnString()
    {
        // Arrange
        var configValue = ConfigurationValue.Create("test", ConfigurationValueType.String);

        // Act
        var result = configValue.GetTypedValue<string>();

        // Assert
        result.Should().Be("test");
    }

    [Fact]
    public void GetTypedValue_WithIntegerType_ShouldReturnInteger()
    {
        // Arrange
        var configValue = ConfigurationValue.Create("123", ConfigurationValueType.Integer);

        // Act
        var result = configValue.GetTypedValue<int>();

        // Assert
        result.Should().Be(123);
    }

    [Fact]
    public void GetTypedValue_WithBooleanType_ShouldReturnBoolean()
    {
        // Arrange
        var configValue = ConfigurationValue.Create("true", ConfigurationValueType.Boolean);

        // Act
        var result = configValue.GetTypedValue<bool>();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void GetTypedValue_WithDecimalType_ShouldReturnDecimal()
    {
        // Arrange
        var configValue = ConfigurationValue.Create("123.45", ConfigurationValueType.Decimal);

        // Act
        var result = configValue.GetTypedValue<decimal>();

        // Assert
        result.Should().Be(123.45m);
    }

    [Fact]
    public void GetTypedValue_WithDateTimeType_ShouldReturnDateTime()
    {
        // Arrange
        var dateString = "2023-12-01T10:30:00";
        var configValue = ConfigurationValue.Create(dateString, ConfigurationValueType.DateTime);

        // Act
        var result = configValue.GetTypedValue<DateTime>();

        // Assert
        result.Should().Be(DateTime.Parse(dateString));
    }

    [Fact]
    public void ImplicitConversion_ToString_ShouldWork()
    {
        // Arrange
        var configValue = ConfigurationValue.Create("test", ConfigurationValueType.String);

        // Act
        string stringValue = configValue;

        // Assert
        stringValue.Should().Be("test");
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        // Arrange
        var configValue = ConfigurationValue.Create("test", ConfigurationValueType.String);

        // Act
        var result = configValue.ToString();

        // Assert
        result.Should().Be("test");
    }
}
