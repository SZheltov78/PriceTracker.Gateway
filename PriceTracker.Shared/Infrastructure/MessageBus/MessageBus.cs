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

            if (message is IHasCorrelationId hasCorrelationId && hasCorrelationId != null)
            {
                properties.CorrelationId = hasCorrelationId.CorrelationId;                
            }

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

            var correlationId = result.BasicProperties?.CorrelationId;                
            if(message is IHasCorrelationId hasCorrelationId && correlationId != null)
            {
                hasCorrelationId.CorrelationId = correlationId;
            }

            _channel.BasicAck(result.DeliveryTag, false);

            return Task.FromResult(message);
        }
        catch (Exception)
        {
            _channel.BasicNack(result.DeliveryTag, false, true);
            throw;
        }
    }

    public async Task<R> CallAsync<T, R>(T request, string requestQueue, string responseQueue, CancellationToken ct = default)
    {
        // Декларируем очереди
        _channel.QueueDeclare(
            queue: requestQueue,
            durable: true,
            exclusive: false,
            autoDelete: false);

        _channel.QueueDeclare(
            queue: responseQueue,
            durable: true,
            exclusive: false,
            autoDelete: false);

        var correlationId = Guid.NewGuid().ToString();
        var tcs = new TaskCompletionSource<R>();

        // Создаем consumer для ответов
        var consumer = new AsyncEventingBasicConsumer(_channel);
        string? consumerTag = null;

        consumer.Received += async (model, ea) =>
        {
            // Проверяем, что это ответ на наш запрос
            if (ea.BasicProperties.CorrelationId == correlationId)
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var json = Encoding.UTF8.GetString(body);
                    var response = JsonSerializer.Deserialize<R>(json, _jsonOptions);

                    _channel.BasicAck(ea.DeliveryTag, false);

                    // Отписываемся после получения
                    if (consumerTag != null)
                    {
                        _channel.BasicCancel(consumerTag);
                    }

                    tcs.TrySetResult(response!);
                }
                catch (Exception ex)
                {
                    _channel.BasicNack(ea.DeliveryTag, false, true);
                    tcs.TrySetException(ex);
                }
            }
            else
            {
                // Чужой correlationId - возвращаем в очередь
                _channel.BasicNack(ea.DeliveryTag, false, true);
            }

            await Task.CompletedTask;
        };

        // Начинаем слушать очередь ответов
        consumerTag = _channel.BasicConsume(
            queue: responseQueue,
            autoAck: false,
            consumer: consumer);

        // Отправляем запрос
        var requestJson = JsonSerializer.Serialize(request, _jsonOptions);
        var requestBody = Encoding.UTF8.GetBytes(requestJson);

        var props = _channel.CreateBasicProperties();
        props.CorrelationId = correlationId;
        props.ReplyTo = responseQueue;
        props.Persistent = true;

        _channel.BasicPublish(
            exchange: "",
            routingKey: requestQueue,
            basicProperties: props,
            body: requestBody);

        // Ждем ответ с таймаутом
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(TimeSpan.FromSeconds(30));

        using var registration = cts.Token.Register(() =>
        {
            if (consumerTag != null)
            {
                _channel.BasicCancel(consumerTag);
            }
            tcs.TrySetCanceled();
        });

        return await tcs.Task;
    }

    public void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
        _channel?.Dispose();
        _connection?.Dispose();
    }
}