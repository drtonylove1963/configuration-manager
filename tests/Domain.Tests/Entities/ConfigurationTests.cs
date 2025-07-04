using Domain.Entities;
using Domain.ValueObjects;
using FluentAssertions;

namespace Domain.Tests.Entities;

public class ConfigurationTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateConfiguration()
    {
        // Arrange
        var key = "TestKey";
        var value = "TestValue";
        var valueType = ConfigurationValueType.String;
        var description = "Test Description";
        var applicationId = Guid.NewGuid();
        var environmentId = Guid.NewGuid();
        var createdBy = "testuser";

        // Act
        var configuration = new Configuration(key, value, valueType, description, applicationId, environmentId, createdBy);

        // Assert
        configuration.Key.Value.Should().Be(key);
        configuration.Value.Value.Should().Be(value);
        configuration.Value.Type.Should().Be(valueType);
        configuration.Description.Should().Be(description);
        configuration.ApplicationId.Should().Be(applicationId);
        configuration.EnvironmentId.Should().Be(environmentId);
        configuration.CreatedBy.Should().Be(createdBy);
        configuration.IsActive.Should().BeTrue();
        configuration.Version.Should().Be(1);
        configuration.IsEncrypted.Should().BeFalse();
        configuration.IsRequired.Should().BeFalse();
    }

    [Fact]
    public void Constructor_WithOptionalParameters_ShouldSetCorrectValues()
    {
        // Arrange
        var key = "TestKey";
        var value = "TestValue";
        var valueType = ConfigurationValueType.String;
        var description = "Test Description";
        var applicationId = Guid.NewGuid();
        var environmentId = Guid.NewGuid();
        var groupId = Guid.NewGuid();
        var createdBy = "testuser";

        // Act
        var configuration = new Configuration(
            key, value, valueType, description, applicationId, environmentId, createdBy,
            groupId, isEncrypted: true, isRequired: true, defaultValue: "default");

        // Assert
        configuration.GroupId.Should().Be(groupId);
        configuration.IsEncrypted.Should().BeTrue();
        configuration.IsRequired.Should().BeTrue();
        configuration.DefaultValue.Should().Be("default");
    }

    [Fact]
    public void UpdateValue_ShouldUpdateValueAndIncrementVersion()
    {
        // Arrange
        var configuration = CreateTestConfiguration();
        var newValue = "NewValue";
        var updatedBy = "updater";

        // Act
        configuration.UpdateValue(newValue, ConfigurationValueType.String, updatedBy, "Test reason");

        // Assert
        configuration.Value.Value.Should().Be(newValue);
        configuration.Version.Should().Be(2);
        configuration.UpdatedBy.Should().Be(updatedBy);
        configuration.UpdatedAt.Should().NotBeNull();
        configuration.History.Should().HaveCount(1);
        configuration.History.First().NewValue.Should().Be(newValue);
        configuration.History.First().ChangeReason.Should().Be("Test reason");
    }

    [Fact]
    public void UpdateDetails_ShouldUpdateProperties()
    {
        // Arrange
        var configuration = CreateTestConfiguration();
        var newDescription = "Updated Description";
        var newGroupId = Guid.NewGuid();
        var updatedBy = "updater";

        // Act
        configuration.UpdateDetails(newDescription, updatedBy, newGroupId, true, "new default");

        // Assert
        configuration.Description.Should().Be(newDescription);
        configuration.GroupId.Should().Be(newGroupId);
        configuration.IsRequired.Should().BeTrue();
        configuration.DefaultValue.Should().Be("new default");
        configuration.UpdatedBy.Should().Be(updatedBy);
    }

    [Fact]
    public void ToggleEncryption_ShouldToggleEncryptionFlag()
    {
        // Arrange
        var configuration = CreateTestConfiguration();
        var originalEncryption = configuration.IsEncrypted;
        var updatedBy = "updater";

        // Act
        configuration.ToggleEncryption(updatedBy);

        // Assert
        configuration.IsEncrypted.Should().Be(!originalEncryption);
        configuration.UpdatedBy.Should().Be(updatedBy);
    }

    [Fact]
    public void Activate_ShouldSetIsActiveToTrue()
    {
        // Arrange
        var configuration = CreateTestConfiguration();
        configuration.Deactivate("deactivator");
        var updatedBy = "activator";

        // Act
        configuration.Activate(updatedBy);

        // Assert
        configuration.IsActive.Should().BeTrue();
        configuration.UpdatedBy.Should().Be(updatedBy);
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveToFalse()
    {
        // Arrange
        var configuration = CreateTestConfiguration();
        var updatedBy = "deactivator";

        // Act
        configuration.Deactivate(updatedBy);

        // Assert
        configuration.IsActive.Should().BeFalse();
        configuration.UpdatedBy.Should().Be(updatedBy);
    }

    [Fact]
    public void Delete_ShouldMarkAsDeleted()
    {
        // Arrange
        var configuration = CreateTestConfiguration();
        var deletedBy = "deleter";

        // Act
        configuration.Delete(deletedBy);

        // Assert
        configuration.IsDeleted.Should().BeTrue();
        configuration.DeletedBy.Should().Be(deletedBy);
        configuration.DeletedAt.Should().NotBeNull();
    }

    private static Configuration CreateTestConfiguration()
    {
        return new Configuration(
            "TestKey",
            "TestValue",
            ConfigurationValueType.String,
            "Test Description",
            Guid.NewGuid(), // ApplicationId
            Guid.NewGuid(), // EnvironmentId
            "testuser"
        );
    }
}
