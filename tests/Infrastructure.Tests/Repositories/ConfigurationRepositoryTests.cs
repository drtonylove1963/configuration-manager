using Domain.Entities;
using Domain.ValueObjects;
using FluentAssertions;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace Infrastructure.Tests.Repositories;

public class ConfigurationRepositoryTests : IDisposable
{
    private readonly ConfigurationDbContext _context;
    private readonly ConfigurationRepository _repository;
    private readonly Mock<ILogger<ConfigurationRepository>> _loggerMock;

    public ConfigurationRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ConfigurationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ConfigurationDbContext(options);
        _loggerMock = new Mock<ILogger<ConfigurationRepository>>();
        _repository = new ConfigurationRepository(_context, _loggerMock.Object);
    }

    [Fact]
    public async Task AddAsync_ShouldAddConfigurationToDatabase()
    {
        // Arrange
        var environment = new Domain.Entities.Environment("Test", "Test Environment", "testuser");
        await _context.Environments.AddAsync(environment);
        await _context.SaveChangesAsync();

        var configuration = new Configuration(
            "TestKey",
            "TestValue",
            ConfigurationValueType.String,
            "Test Description",
            environment.Id,
            "testuser");

        // Act
        await _repository.AddAsync(configuration);

        // Assert
        var savedConfiguration = await _context.Configurations.FirstOrDefaultAsync();
        savedConfiguration.Should().NotBeNull();
        savedConfiguration!.Key.Value.Should().Be("TestKey");
        savedConfiguration.Value.Value.Should().Be("TestValue");
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingId_ShouldReturnConfiguration()
    {
        // Arrange
        var environment = new Domain.Entities.Environment("Test", "Test Environment", "testuser");
        await _context.Environments.AddAsync(environment);

        var configuration = new Configuration(
            "TestKey",
            "TestValue",
            ConfigurationValueType.String,
            "Test Description",
            environment.Id,
            "testuser");

        await _context.Configurations.AddAsync(configuration);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(configuration.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Key.Value.Should().Be("TestKey");
        result.Environment.Should().NotBeNull();
        result.Environment.Name.Should().Be("Test");
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistingId_ShouldReturnNull()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid();

        // Act
        var result = await _repository.GetByIdAsync(nonExistingId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByKeyAndEnvironmentAsync_WithExistingKeyAndEnvironment_ShouldReturnConfiguration()
    {
        // Arrange
        var environment = new Domain.Entities.Environment("Test", "Test Environment", "testuser");
        await _context.Environments.AddAsync(environment);

        var configuration = new Configuration(
            "TestKey",
            "TestValue",
            ConfigurationValueType.String,
            "Test Description",
            environment.Id,
            "testuser");

        await _context.Configurations.AddAsync(configuration);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByKeyAndEnvironmentAsync("TestKey", environment.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Key.Value.Should().Be("TestKey");
        result.EnvironmentId.Should().Be(environment.Id);
    }

    [Fact]
    public async Task GetByEnvironmentAsync_ShouldReturnConfigurationsForEnvironment()
    {
        // Arrange
        var environment1 = new Domain.Entities.Environment("Test1", "Test Environment 1", "testuser");
        var environment2 = new Domain.Entities.Environment("Test2", "Test Environment 2", "testuser");
        await _context.Environments.AddRangeAsync(environment1, environment2);

        var config1 = new Configuration("Key1", "Value1", ConfigurationValueType.String, "Desc1", environment1.Id, "testuser");
        var config2 = new Configuration("Key2", "Value2", ConfigurationValueType.String, "Desc2", environment1.Id, "testuser");
        var config3 = new Configuration("Key3", "Value3", ConfigurationValueType.String, "Desc3", environment2.Id, "testuser");

        await _context.Configurations.AddRangeAsync(config1, config2, config3);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByEnvironmentAsync(environment1.Id);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(c => c.EnvironmentId == environment1.Id);
    }

    [Fact]
    public async Task ExistsAsync_WithExistingConfiguration_ShouldReturnTrue()
    {
        // Arrange
        var environment = new Domain.Entities.Environment("Test", "Test Environment", "testuser");
        await _context.Environments.AddAsync(environment);

        var configuration = new Configuration(
            "TestKey",
            "TestValue",
            ConfigurationValueType.String,
            "Test Description",
            environment.Id,
            "testuser");

        await _context.Configurations.AddAsync(configuration);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.ExistsAsync("TestKey", environment.Id);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_WithNonExistingConfiguration_ShouldReturnFalse()
    {
        // Arrange
        var environment = new Domain.Entities.Environment("Test", "Test Environment", "testuser");
        await _context.Environments.AddAsync(environment);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.ExistsAsync("NonExistingKey", environment.Id);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateConfiguration()
    {
        // Arrange
        var environment = new Domain.Entities.Environment("Test", "Test Environment", "testuser");
        await _context.Environments.AddAsync(environment);

        var configuration = new Configuration(
            "TestKey",
            "TestValue",
            ConfigurationValueType.String,
            "Test Description",
            environment.Id,
            "testuser");

        await _context.Configurations.AddAsync(configuration);
        await _context.SaveChangesAsync();

        // Act
        configuration.UpdateValue("UpdatedValue", ConfigurationValueType.String, "updater");
        await _repository.UpdateAsync(configuration);

        // Assert
        var updatedConfiguration = await _context.Configurations.FirstAsync();
        updatedConfiguration.Value.Value.Should().Be("UpdatedValue");
        updatedConfiguration.UpdatedBy.Should().Be("updater");
        updatedConfiguration.Version.Should().Be(2);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
