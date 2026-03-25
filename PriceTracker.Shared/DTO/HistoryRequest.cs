using PriceTracker.Shared.Infrastructure.MessageBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PriceTracker.Shared.DTO
{
    public class HistoryRequest: IHasCorrelationId
    {
        public string CorrelationId { get; set; }
        public string Email { get; set; }
        public int Take { get; set; }
    }
}
