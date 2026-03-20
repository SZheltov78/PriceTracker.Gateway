using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PriceTracker.Shared.DTO
{
    public class BasicGetResult<T>
    {
        public T Message { get; set; }
        public string? CorrelationId { get; set; }
        public string? ReplyTo { get; set; }
        public ulong DeliveryTag { get; set; }
    }
}
