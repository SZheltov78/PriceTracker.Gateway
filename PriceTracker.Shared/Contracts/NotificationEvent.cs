using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PriceTracker.Shared.Contracts
{
    public class NotificationEvent
    {
        public string ProductName { get; set; }
        public decimal Price { get; set; }
        public string Email { get; set; }
    }
}
