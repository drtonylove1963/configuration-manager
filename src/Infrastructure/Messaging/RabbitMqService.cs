using Domain.Events;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace Infrastructure.Messaging;

public class RabbitMqSettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public string ExchangeName { get; set; } = "configuration-manager";
    public string ConfigurationChangedQueue { get; set; } = "configuration-changed";
    public string ConfigurationCreatedQueue { get; set; } = "configuration-created";
    public string ConfigurationDeletedQueue { get; set; } = "configuration-deleted";
}

public interface IMessageBusService
{
    Task PublishConfigurationChangedAsync(ConfigurationChangedEvent eventData, CancellationToken cancellationToken = default);
    Task PublishConfigurationCreatedAsync(ConfigurationCreatedEvent eventData, CancellationToken cancellationToken = default);
    Task PublishConfigurationDeletedAsync(ConfigurationDeletedEvent eventData, CancellationToken cancellationToken = default);
}

public class RabbitMqService : IMessageBusService, IDisposable
{
    private readonly RabbitMqSettings _settings;
    private readonly ILogger<RabbitMqService> _logger;
    private readonly IConnection _connection;
    private readonly IModel _channel;

    public RabbitMqService(IOptions<RabbitMqSettings> settings, ILogger<RabbitMqService> logger)
    {
        _settings = settings.Value;
        _logger = logger;

        try
        {
            var factory = new ConnectionFactory();
            factory.Uri = new Uri(_settings.ConnectionString);
            
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            SetupExchangeAndQueues();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize RabbitMQ connection");
            throw;
        }
    }

    private void SetupExchangeAndQueues()
    {
        // Declare exchange
        _channel.ExchangeDeclare(
            exchange: _settings.ExchangeName,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false);

        // Declare queues
        var queues = new[]
        {
            _settings.ConfigurationChangedQueue,
            _settings.ConfigurationCreatedQueue,
            _settings.ConfigurationDeletedQueue
        };

        foreach (var queue in queues)
        {
            _channel.QueueDeclare(
                queue: queue,
                durable: true,
                exclusive: false,
                autoDelete: false);

            // Bind queue to exchange with routing key
            var routingKey = queue.Replace("-", ".");
            _channel.QueueBind(
                queue: queue,
                exchange: _settings.ExchangeName,
                routingKey: routingKey);
        }

        _logger.LogInformation("RabbitMQ exchange and queues setup completed");
    }

    public async Task PublishConfigurationChangedAsync(ConfigurationChangedEvent eventData, CancellationToken cancellationToken = default)
    {
        await PublishEventAsync("configuration.changed", eventData, _settings.ConfigurationChangedQueue, cancellationToken);
    }

    public async Task PublishConfigurationCreatedAsync(ConfigurationCreatedEvent eventData, CancellationToken cancellationToken = default)
    {
        await PublishEventAsync("configuration.created", eventData, _settings.ConfigurationCreatedQueue, cancellationToken);
    }

    public async Task PublishConfigurationDeletedAsync(ConfigurationDeletedEvent eventData, CancellationToken cancellationToken = default)
    {
        await PublishEventAsync("configuration.deleted", eventData, _settings.ConfigurationDeletedQueue, cancellationToken);
    }

    private async Task PublishEventAsync<T>(string routingKey, T eventData, string queueName, CancellationToken cancellationToken = default)
    {
        try
        {
            var message = JsonSerializer.Serialize(eventData, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var body = Encoding.UTF8.GetBytes(message);

            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true;
            properties.MessageId = Guid.NewGuid().ToString();
            properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            properties.ContentType = "application/json";
            properties.Type = typeof(T).Name;

            _channel.BasicPublish(
                exchange: _settings.ExchangeName,
                routingKey: routingKey,
                basicProperties: properties,
                body: body);

            _logger.LogDebug("Published event {EventType} to queue {Queue} with routing key {RoutingKey}", 
                typeof(T).Name, queueName, routingKey);

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing event {EventType} to queue {Queue}", typeof(T).Name, queueName);
            throw;
        }
    }

    public void Dispose()
    {
        try
        {
            _channel?.Close();
            _channel?.Dispose();
            _connection?.Close();
            _connection?.Dispose();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disposing RabbitMQ connection");
        }
    }
}
