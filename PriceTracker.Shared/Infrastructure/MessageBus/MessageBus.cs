using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Microsoft.Extensions.Options;
using PriceTracker.Shared.Infrastructure.MessageBus;
using System.Text;
using System.Text.Json;

namespace PriceTracker.Shared.Messaging;

public class RabbitMqBus : IMessageBus, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly JsonSerializerOptions _jsonOptions;

    public RabbitMqBus(IOptions<RabbitMqOptions> options)
    {
        var factory = new ConnectionFactory
        {
            HostName = options.Value.Host,
            Port = options.Value.Port,
            UserName = options.Value.UserName,
            Password = options.Value.Password,
            DispatchConsumersAsync = true
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public async Task PublishAsync<T>(T message, string queueName, CancellationToken ct = default)
    {
        await Task.Run(() =>
        {
            _channel.QueueDeclare(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false);

            var json = JsonSerializer.Serialize(message, _jsonOptions);
            var body = Encoding.UTF8.GetBytes(json);

            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true;

            _channel.BasicPublish(
                exchange: "",
                routingKey: queueName,
                basicProperties: properties,
                body: body);
        }, ct);
    }

    public Task<T?> ConsumeAsync<T>(string queueName, CancellationToken ct = default)
    {        
        _channel.QueueDeclare(
            queue: queueName,
            durable: true,
            exclusive: false,
            autoDelete: false);
        
        var result = _channel.BasicGet(queueName, autoAck: false);

        if (result == null)
        {
            return Task.FromResult<T?>(default);
        }

        try
        {
            var body = result.Body.ToArray();
            var json = Encoding.UTF8.GetString(body);
            var message = JsonSerializer.Deserialize<T>(json, _jsonOptions);
                        
            _channel.BasicAck(result.DeliveryTag, false);

            return Task.FromResult(message);
        }
        catch (Exception)
        {            
            _channel.BasicNack(result.DeliveryTag, false, true);
            throw;
        }
    }

    public void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
        _channel?.Dispose();
        _connection?.Dispose();
    }
}