using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PriceTracker.Shared.Infrastructure.MessageBus
{
    public interface IHasCorrelationId
    {
        string CorrelationId { get; set; }
    }
}
