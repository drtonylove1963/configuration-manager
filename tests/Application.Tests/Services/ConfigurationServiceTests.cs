using Application.DTOs.Configuration;
using Application.Services;
using AutoMapper;
using Domain.Entities;
using Domain.Exceptions;
using Domain.Repositories;
using Domain.ValueObjects;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace Application.Tests.Services;

public class ConfigurationServiceTests
{
    private readonly Mock<IConfigurationRepository> _configurationRepositoryMock;
    private readonly Mock<IEnvironmentRepository> _environmentRepositoryMock;
    private readonly Mock<IConfigurationGroupRepository> _groupRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<ConfigurationService>> _loggerMock;
    private readonly ConfigurationService _service;

    public ConfigurationServiceTests()
    {
        _configurationRepositoryMock = new Mock<IConfigurationRepository>();
        _environmentRepositoryMock = new Mock<IEnvironmentRepository>();
        _groupRepositoryMock = new Mock<IConfigurationGroupRepository>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<ConfigurationService>>();

        _service = new ConfigurationService(
            _configurationRepositoryMock.Object,
            _environmentRepositoryMock.Object,
            _groupRepositoryMock.Object,
            _mapperMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingId_ShouldReturnConfiguration()
    {
        // Arrange
        var configurationId = Guid.NewGuid();
        var configuration = CreateTestConfiguration(configurationId);
        var configurationDto = CreateTestConfigurationDto(configurationId);

        _configurationRepositoryMock
            .Setup(x => x.GetByIdAsync(configurationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(configuration);

        _mapperMock
            .Setup(x => x.Map<ConfigurationDto>(configuration))
            .Returns(configurationDto);

        // Act
        var result = await _service.GetByIdAsync(configurationId);

        // Assert
        result.Should().NotBeNull();
        result.Should().Be(configurationDto);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistingId_ShouldReturnNull()
    {
        // Arrange
        var configurationId = Guid.NewGuid();

        _configurationRepositoryMock
            .Setup(x => x.GetByIdAsync(configurationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Configuration?)null);

        // Act
        var result = await _service.GetByIdAsync(configurationId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_WithValidData_ShouldCreateConfiguration()
    {
        // Arrange
        var environmentId = Guid.NewGuid();
        var environment = CreateTestEnvironment(environmentId);
        var createDto = new CreateConfigurationDto(
            "TestKey",
            "TestValue",
            ConfigurationValueType.String,
            "Test Description",
            Guid.NewGuid(), // ApplicationId
            environmentId);

        var configuration = CreateTestConfiguration();
        var configurationDto = CreateTestConfigurationDto();

        _environmentRepositoryMock
            .Setup(x => x.GetByIdAsync(environmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(environment);

        _configurationRepositoryMock
            .Setup(x => x.ExistsAsync(createDto.Key, environmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _configurationRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Configuration>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mapperMock
            .Setup(x => x.Map<ConfigurationDto>(It.IsAny<Configuration>()))
            .Returns(configurationDto);

        // Act
        var result = await _service.CreateAsync(createDto, "testuser");

        // Assert
        result.Should().NotBeNull();
        result.Should().Be(configurationDto);
        _configurationRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Configuration>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithNonExistingEnvironment_ShouldThrowEnvironmentNotFoundException()
    {
        // Arrange
        var applicationId = Guid.NewGuid();
        var environmentId = Guid.NewGuid();
        var createDto = new CreateConfigurationDto(
            "TestKey",
            "TestValue",
            ConfigurationValueType.String,
            "Test Description",
            applicationId,
            environmentId);

        _environmentRepositoryMock
            .Setup(x => x.GetByIdAsync(environmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Domain.Entities.Environment?)null);

        // Act & Assert
        var action = async () => await _service.CreateAsync(createDto, "testuser");
        await action.Should().ThrowAsync<EnvironmentNotFoundException>();
    }

    [Fact]
    public async Task CreateAsync_WithExistingKey_ShouldThrowConfigurationAlreadyExistsException()
    {
        // Arrange
        var applicationId = Guid.NewGuid();
        var environmentId = Guid.NewGuid();
        var environment = CreateTestEnvironment(environmentId);
        var createDto = new CreateConfigurationDto(
            "TestKey",
            "TestValue",
            ConfigurationValueType.String,
            "Test Description",
            applicationId,
            environmentId);

        _environmentRepositoryMock
            .Setup(x => x.GetByIdAsync(environmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(environment);

        _configurationRepositoryMock
            .Setup(x => x.ExistsAsync(createDto.Key, environmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act & Assert
        var action = async () => await _service.CreateAsync(createDto, "testuser");
        await action.Should().ThrowAsync<ConfigurationAlreadyExistsException>();
    }

    [Fact]
    public async Task UpdateAsync_WithValidData_ShouldUpdateConfiguration()
    {
        // Arrange
        var configurationId = Guid.NewGuid();
        var configuration = CreateTestConfiguration(configurationId);
        var updateDto = new UpdateConfigurationDto(
            "UpdatedValue",
            ConfigurationValueType.String,
            "Updated Description");

        var configurationDto = CreateTestConfigurationDto(configurationId);

        _configurationRepositoryMock
            .Setup(x => x.GetByIdAsync(configurationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(configuration);

        _configurationRepositoryMock
            .Setup(x => x.UpdateAsync(configuration, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mapperMock
            .Setup(x => x.Map<ConfigurationDto>(configuration))
            .Returns(configurationDto);

        // Act
        var result = await _service.UpdateAsync(configurationId, updateDto, "testuser");

        // Assert
        result.Should().NotBeNull();
        result.Should().Be(configurationDto);
        _configurationRepositoryMock.Verify(x => x.UpdateAsync(configuration, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WithNonExistingConfiguration_ShouldThrowConfigurationNotFoundException()
    {
        // Arrange
        var configurationId = Guid.NewGuid();
        var updateDto = new UpdateConfigurationDto(
            "UpdatedValue",
            ConfigurationValueType.String,
            "Updated Description");

        _configurationRepositoryMock
            .Setup(x => x.GetByIdAsync(configurationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Configuration?)null);

        // Act & Assert
        var action = async () => await _service.UpdateAsync(configurationId, updateDto, "testuser");
        await action.Should().ThrowAsync<ConfigurationNotFoundException>();
    }

    private static Configuration CreateTestConfiguration(Guid? id = null)
    {
        var configuration = new Configuration(
            "TestKey",
            "TestValue",
            ConfigurationValueType.String,
            "Test Description",
            Guid.NewGuid(), // ApplicationId
            Guid.NewGuid(), // EnvironmentId
            "testuser");

        if (id.HasValue)
        {
            // Use reflection to set the Id for testing purposes
            var idProperty = typeof(Configuration).GetProperty("Id");
            idProperty?.SetValue(configuration, id.Value);
        }

        return configuration;
    }

    private static ConfigurationDto CreateTestConfigurationDto(Guid? id = null)
    {
        return new ConfigurationDto(
            id ?? Guid.NewGuid(),
            "TestKey",
            "TestValue",
            ConfigurationValueType.String,
            "Test Description",
            Guid.NewGuid(), // ApplicationId
            "TestApplication", // ApplicationName
            Guid.NewGuid(), // EnvironmentId
            "TestEnvironment", // EnvironmentName
            null,
            null,
            false,
            false,
            null,
            true,
            1,
            DateTime.UtcNow,
            null,
            "testuser",
            null);
    }

    private static Domain.Entities.Environment CreateTestEnvironment(Guid? id = null)
    {
        var environment = new Domain.Entities.Environment("TestEnvironment", "Test Description", "testuser");

        if (id.HasValue)
        {
            // Use reflection to set the Id for testing purposes
            var idProperty = typeof(Domain.Entities.Environment).GetProperty("Id");
            idProperty?.SetValue(environment, id.Value);
        }

        return environment;
    }
}
