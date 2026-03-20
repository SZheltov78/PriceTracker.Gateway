using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PriceTracker.Shared.Infrastructure.MessageBus
{
    public interface IMessageBus
    {
        Task PublishAsync<T>(T message, string queueName, CancellationToken ct = default);
        Task<T?> ConsumeAsync<T>(string queueName, CancellationToken ct = default);
        Task<R> CallAsync<T, R>(T request, string requestQueue, string responseQueue, CancellationToken ct = default);
    }
}
